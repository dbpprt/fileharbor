package services

import (
	"log"

	"github.com/dennisbappert/fileharbor/common"
	"github.com/jmoiron/sqlx"
	uuid "github.com/satori/go.uuid"
)

type CollectionService struct {
	database      *sqlx.DB
	configuration *common.Configuration
	Services      *Services
}

func NewCollectionService(configuration *common.Configuration, database *sqlx.DB, services *Services) *CollectionService {
	service := &CollectionService{database: database, configuration: configuration, Services: services}
	return service
}

func (service *CollectionService) Create() (string, error) {
	id := uuid.NewV4().String()

	log.Println("creating new collection", id)
	tx := service.database.MustBegin()
	tx.MustExec("INSERT INTO collections (id, quota) VALUES($1, $2)", id, service.configuration.DefaultQuota)
	err := tx.Commit()

	if err != nil {
		log.Println("error while executing transaction", err)
		return "", err
	}

	log.Println("successfully created collection", id, service.configuration.DefaultQuota)
	return id, nil
}

func (service *CollectionService) AssignUser(userId string, id string) error {
	log.Println("assign user to collection", userId, id)

	// TODO: should not always be true?
	isDefault := true

	tx := service.database.MustBegin()
	// TODO: error handling
	tx.MustExec("INSERT INTO user_collection_mappings (user_id, collection_id, is_default) VALUES ($1, $2, $3)", userId, id, isDefault)
	err := tx.Commit()

	if err != nil {
		log.Println("error while executing transaction", err)
		return err
	}

	return nil
}
