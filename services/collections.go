package services

import (
	"database/sql"

	"github.com/dennisbappert/fileharbor/common"
	"github.com/dennisbappert/fileharbor/models"
	"github.com/jmoiron/sqlx"
	uuid "github.com/satori/go.uuid"
)

type CollectionService struct {
	Service
}

func NewCollectionService(configuration *common.Configuration, database *sqlx.DB, services *ServiceContext) *CollectionService {
	service := &CollectionService{Service{database: database, configuration: configuration, ServiceContext: services}}
	return service
}

func (service *CollectionService) Exists(id string) (bool, error) {
	collection := models.CollectionEntity{}
	service.log.Println("looking up collection", id)
	err := service.database.Get(&collection, "SELECT * FROM collections where id=$1", id)

	// TODO: thhis looks like bullshit, there should be a better way
	if err != nil && err == sql.ErrNoRows {
		service.log.Println("collection is not existing")
		return false, nil
	} else if err != nil {
		return true, err
	}

	service.log.Println("collection is existing", collection)
	return true, nil
}

func (service *CollectionService) Create(tx *sqlx.Tx) (string, error) {
	id := uuid.NewV4().String()

	service.log.Println("creating new collection", id)
	commit := false
	if tx == nil {
		tx = service.database.MustBegin()
		commit = true
	}
	_, err := tx.Exec("INSERT INTO collections (id, quota) VALUES($1, $2)", id, service.configuration.DefaultQuota)

	if err != nil {
		service.log.Println("unable to create collection", err)

		if commit == true {
			tx.Rollback()
		}

		return "", err
	}

	service.log.Println("creating a bucket for the new collection")
	err = service.StorageService.CreateBucket(id)

	if err != nil {
		service.log.Println("unable to create bucket for user,")

		if commit == true {
			tx.Rollback()
		}

		return "", err
	}
	service.log.Println("created a new bucket", id)

	if commit == true {
		err := tx.Commit()

		if err != nil {
			service.log.Println("error while executing transaction", err)
			return "", err
		}
	} else {
		service.log.Println("transaction passed to function - skipping commit")
	}

	service.log.Println("successfully created collection", id, service.configuration.DefaultQuota)
	return id, nil
}

func (service *CollectionService) AssignUser(userId string, id string, tx *sqlx.Tx) error {
	service.log.Println("assign user to collection", userId, id)

	// TODO: should not always be true?
	isDefault := true

	commit := false
	if tx == nil {
		tx = service.database.MustBegin()
		commit = true
	}

	_, err := tx.Exec("INSERT INTO user_collection_mappings (user_id, collection_id, is_default) VALUES ($1, $2, $3)", userId, id, isDefault)

	if err != nil {
		service.log.Println("unable to create collection", err)

		if commit == true {
			tx.Rollback()
		}

		return err
	}

	if commit == true {
		err := tx.Commit()

		if err != nil {
			service.log.Println("error while executing transaction", err)
			return err
		}
	} else {
		service.log.Println("transaction passed to function - skipping commit")
	}

	return nil
}

func (service *CollectionService) MyCollections() (*[]models.CollectionEntity, error) {
	service.log.Println("trying to fetch my collections")

	if err := service.AuthorizationService.EnsureLoggedInUser(); err != nil {
		service.log.Println("unable to fetch my collections - no valid user identity", err)
		return nil, err
	}

	collections := []models.CollectionEntity{}
	userID := service.Environment.CurrentUserId

	err := service.database.Select(&collections, `
		SELECT
			collections.*,
			user_collection_mappings.is_default "user_collection_mapping.is_default"
		FROM collections, user_collection_mappings
		WHERE collections.id = user_collection_mappings.collection_id AND user_collection_mappings.user_id = $1
	`, userID)

	if err != nil {
		return nil, err
	}

	return &collections, nil
}
