package services

import (
	"encoding/json"
	"io/ioutil"
	"os"

	"path"

	"strings"

	"github.com/dennisbappert/fileharbor/common"
	"github.com/jmoiron/sqlx"
)

type CollectionTemplateService struct {
	Service
}

type CollectionTemplate struct {
	ID          string
	Name        string
	Description string
	Language    string
	Columns     []Column
}

func NewCollectionTemplateService(configuration *common.Configuration, database *sqlx.DB, services *ServiceContext) *CollectionTemplateService {
	service := &CollectionTemplateService{Service{database: database, configuration: configuration, ServiceContext: services}}
	return service
}

func (service *CollectionTemplateService) GetAvaliableTemplates() (*[]CollectionTemplate, error) {
	service.log.Println("searching all avaliable templates in template folder", service.configuration.TemplateFolder)

	// this strcuts are only required for parsing the template files, is inline them valid?
	type templateDefinition struct {
		ID                  string   `json:"id"`
		Name                string   `json:"name"`
		Description         string   `json:"description"`
		Language            string   `json:"-"` // TODO: implement this from file name of template
		IncludeColumns      []string `json:"include_columns"`
		IncludeContentTypes []string `json:"include_contenttypes"`
	}

	type columnDefinition struct {
		ID          string          `json:"id"`
		Name        string          `json:"name"`
		Description string          `json:"description"`
		Group       string          `json:"group"`
		Type        string          `json:"type"`
		Sealed      bool            `json:"sealed"`
		Settings    json.RawMessage `json:"settings"`
	}

	// the result set
	var results []CollectionTemplate

	templatePath := service.configuration.TemplateFolder
	entries, err := ioutil.ReadDir(templatePath)

	if err != nil {
		service.log.Println("unable to enumerate template folder", err)
		return nil, err
	}

	for _, entry := range entries {
		if !entry.IsDir() {
			continue
		}

		folder := path.Join(templatePath, entry.Name())
		files, err := ioutil.ReadDir(folder)

		if err != nil {
			service.log.Println("unable to process folder, skipping..", folder)
			continue
		}

		for _, entry := range files {
			filePath := path.Join(folder, entry.Name())

			// normalizing the filename
			fileName := strings.ToLower(entry.Name())

			if strings.HasPrefix(fileName, "template.") {
				service.log.Println("found template definition file", filePath)
				fileLanguage := strings.Replace(fileName, "template.", "", 1)
				fileLanguage = strings.Replace(fileLanguage, ".json", "", 1)

				service.log.Println("file has the following language", fileLanguage)

				// try opening and parsing the json file
				file, err := os.Open(filePath) // TODO: is open only opeing with flag read? otherwise it may files while concurrently beeing accessed

				if err != nil {
					service.log.Println("unable to open file - skipping...", err)
					continue
				}

				templateDefinition := &templateDefinition{}
				decoder := json.NewDecoder(file)
				err = decoder.Decode(&templateDefinition)

				if err != nil {
					service.log.Println("unable to parse file - invalid template - skipping...", err)
					continue
				}

				service.log.Println("loaded template", templateDefinition)
				result := &CollectionTemplate{
					ID:          templateDefinition.ID,
					Name:        templateDefinition.Name,
					Description: templateDefinition.Description,
					Language:    fileLanguage,
				}

				hasErrors := false

				// parsing site columns
				for _, include := range templateDefinition.IncludeColumns {
					service.log.Println("processing include", include)
					includePath := path.Join(folder, include)
					service.log.Println("included file is", includePath)

					file, err := os.Open(includePath)
					if err != nil {
						service.log.Println("unable to open file", includePath)
						hasErrors = true
						break
					}

					columnDefinitions := make([]columnDefinition, 0)
					decoder = json.NewDecoder(file)
					err = decoder.Decode(&columnDefinitions)

					if err != nil {
						service.log.Println("unable to parse file - invalid template - skipping...", err)
						hasErrors = true
						break
					}

					service.log.Println("successfully parsed columns include", columnDefinitions)

					for _, columnDefinition := range columnDefinitions {
						service.log.Println("processing column", columnDefinition)

						column := &Column{
							ID:          columnDefinition.ID,
							Name:        columnDefinition.Name,
							Description: columnDefinition.Description,
							Group:       columnDefinition.Group,
							Type:        columnDefinition.Type,
							Sealed:      columnDefinition.Sealed,
							Settings:    columnDefinition.Settings, // TODO: better use a string?
						}

						result.Columns = append(result.Columns, *column)
					}

					if hasErrors {
						service.log.Println("there were any errors while parsing the includes - skipping...")
						continue
					}

					results = append(results, *result)
				}
			}

			service.log.Println("finished processing file", filePath)
		}
	}

	service.log.Println(results)

	return &results, nil
}
