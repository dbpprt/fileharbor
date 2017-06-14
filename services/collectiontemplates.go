package services

import (
	"encoding/json"
	"io/ioutil"
	"log"
	"os"

	"path"

	"strings"

	"github.com/dennisbappert/fileharbor/common"
	"github.com/jmoiron/sqlx"
)

type CollectionTemplateService struct {
	Service
}

type templateDefinition struct {
	Title               string   `json:"title"`
	Description         string   `json:"description"`
	IncludeColumns      []string `json:"include_columns"`
	IncludeContentTypes []string `json:"include_contenttypes"`
}

type CollectionTemplate struct {
	Title       string
	Description string
	Language    string
}

func NewCollectionTemplateService(configuration *common.Configuration, database *sqlx.DB, services *Services) *CollectionTemplateService {
	service := &CollectionTemplateService{Service{database: database, configuration: configuration, Services: services}}
	return service
}

func (service *CollectionTemplateService) GetAvaliableTemplates(language string) error {
	log.Println("searching all avaliable templates in template folder", service.configuration.TemplateFolder)

	// we only want to work with lower-case names
	language = strings.ToLower(language)

	// the result set
	var results []CollectionTemplate

	templatePath := service.configuration.TemplateFolder
	entries, err := ioutil.ReadDir(templatePath)

	if err != nil {
		log.Println("unable to enumerate template folder", err)
		return err
	}

	for _, entry := range entries {
		if !entry.IsDir() {
			continue
		}

		folder := path.Join(templatePath, entry.Name())
		files, err := ioutil.ReadDir(folder)

		if err != nil {
			log.Println("unable to process folder, skipping..", folder)
			continue
		}

		for _, entry := range files {
			filePath := path.Join(folder, entry.Name())

			// normalizing the filename
			fileName := strings.ToLower(entry.Name())

			if strings.HasPrefix(fileName, "template.") {
				log.Println("found template definition file", filePath)
				fileLanguage := strings.Replace(fileName, "template.", "", 1)
				fileLanguage = strings.Replace(fileLanguage, ".json", "", 1)

				log.Println("file has the following language", fileLanguage)

				if language == fileLanguage {
					// we found a matching template
					log.Println("found a matching template for given language", filePath)

					// try opening and parsing the json file
					file, err := os.Open(filePath)

					if err != nil {
						log.Println("unable to open file - skipping...", err)
						continue
					}

					templateDefinition := &templateDefinition{}
					decoder := json.NewDecoder(file)
					err = decoder.Decode(&templateDefinition)

					if err != nil {
						log.Println("unable to parse file - invalid template - skipping...", err)
						continue
					}

					log.Println("loaded template", templateDefinition)

					// parsing site columns
					for _, include := range templateDefinition.IncludeColumns {
						log.Println("processing include", include)
						includePath := path.Join(folder, include)
						log.Println("included file is", includePath)
					}

					//results = append(results, *result)
				}
			}

			log.Println("finished processing file", filePath)
		}
	}

	log.Println(results)

	return nil
}
