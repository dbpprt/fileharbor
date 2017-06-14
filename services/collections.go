package services

import (
	"log"

	"github.com/dennisbappert/fileharbor/common"
	"github.com/jmoiron/sqlx"
	uuid "github.com/satori/go.uuid"
)

type CollectionService struct {
	Service
}

func NewCollectionService(configuration *common.Configuration, database *sqlx.DB, services *Services) *CollectionService {
	service := &CollectionService{Service{database: database, configuration: configuration, Services: services}}
	return service
}

func (service *CollectionService) Create(tx *sqlx.Tx) (string, error) {
	id := uuid.NewV4().String()

	log.Println("creating new collection", id)
	commit := false
	if tx == nil {
		tx = service.database.MustBegin()
		commit = true
	}
	_, err := tx.Exec("INSERT INTO collections (id, quota) VALUES($1, $2)", id, service.configuration.DefaultQuota)

	if err != nil {
		log.Println("unable to create collection", err)

		if commit == true {
			tx.Rollback()
		}

		return "", err
	}

	log.Println("creating a bucket for the new collection")
	err = service.StorageService.CreateBucket(id)

	if err != nil {
		log.Println("unable to create bucket for user,")

		if commit == true {
			tx.Rollback()
		}

		return "", err
	}
	log.Println("created a new bucket", id)

	if commit == true {
		err := tx.Commit()

		if err != nil {
			log.Println("error while executing transaction", err)
			return "", err
		}
	} else {
		log.Println("transaction passed to function - skipping commit")
	}

	log.Println("successfully created collection", id, service.configuration.DefaultQuota)
	return id, nil
}

func (service *CollectionService) AssignUser(userId string, id string, tx *sqlx.Tx) error {
	log.Println("assign user to collection", userId, id)

	// TODO: should not always be true?
	isDefault := true

	commit := false
	if tx == nil {
		tx = service.database.MustBegin()
		commit = true
	}
	// TODO: error handling
	_, err := tx.Exec("INSERT INTO user_collection_mappings (user_id, collection_id, is_default) VALUES ($1, $2, $3)", userId, id, isDefault)

	if err != nil {
		log.Println("unable to create collection", err)

		if commit == true {
			tx.Rollback()
		}

		return err
	}

	if commit == true {
		err := tx.Commit()

		if err != nil {
			log.Println("error while executing transaction", err)
			return err
		}
	} else {
		log.Println("transaction passed to function - skipping commit")
	}

	return nil
}
