package services

import (
	"io/ioutil"
	"log"

	"path"

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

func NewCollectionTemplateService(configuration *common.Configuration, database *sqlx.DB, services *Services) *CollectionTemplateService {
	service := &CollectionTemplateService{Service{database: database, configuration: configuration, Services: services}}
	return service
}

func (service *CollectionTemplateService) GetAvaliableTemplates(language string) error {
	log.Println("searching all avaliable templates in template folder", service.configuration.TemplateFolder)

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
			file := path.Join(folder, entry.Name())

			log.Println("processing file", file)
		}
	}

	return nil
}
