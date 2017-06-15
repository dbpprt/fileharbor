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
	// TODO: do we need security verification here?

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

func (service *CollectionService) Get(id string) (*models.CollectionEntity, error) {
	service.log.Println("loading collection", id)

	if err := service.AuthorizationService.EnsureCollectionAccess(id); err != nil {
		service.log.Println("unable to get collection - access denied", err)
		return nil, err
	}

	collection := models.CollectionEntity{}
	err := service.database.Get(&collection, "SELECT * FROM collections where id=$1", id)

	if err != nil {
		service.log.Println("unable to get collection, maybe not existing", err)
		return nil, err
	}

	service.log.Println("collection is existing", collection)
	return &collection, nil
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
		service.log.Println("unexpected error while getting my collections", err)
		return nil, err
	}

	service.log.Println("successfully fetched my collections", collections)
	return &collections, nil
}

func (service *CollectionService) UpdateName(collectionID string, name string, tx *sqlx.Tx) error {
	service.log.Println("trying to update collection name", collectionID, name)

	if err := service.AuthorizationService.EnsureCollectionAccess(collectionID); err != nil {
		service.log.Println("unable to update collection name - access denied", err)
		return err
	}

	if ok, err := service.Exists(collectionID); err != nil {
		service.log.Println("unable to verify wether the collection exists", err)
		return err
	} else if ok == false {
		service.log.Println("collection not found", collectionID)
		return common.NewApplicationError("the desired collection was not found", common.ErrNotFound)
	}

	commit := false
	if tx == nil {
		tx = service.database.MustBegin()
		commit = true
	}

	_, err := tx.Exec("UPDATE collections SET name = $1 WHERE collections.id = $2", name, collectionID)

	if err != nil {
		service.log.Println("unexpected error while updating collection name", err)

		if commit {
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

	service.log.Println("successfully updated collection name to", name)
	return nil
}

func (service *CollectionService) InitializeCollection(collectionID string, templateID string, name *string) error {
	service.log.Println("trying to initialize collection", collectionID, templateID)

	if err := service.AuthorizationService.EnsureCollectionAccess(collectionID); err != nil {
		service.log.Println("unable to initialize collection - access denied", err)
		return err
	}

	if ok, err := service.Exists(collectionID); err != nil {
		service.log.Println("unable to verify wether the collection exists", err)
		return err
	} else if ok == false {
		service.log.Println("collection not found", collectionID)
		return common.NewApplicationError("the desired collection was not found", common.ErrNotFound)
	}

	collection, err := service.Get(collectionID)

	if err != nil {
		service.log.Println("unable to initialize collection, the initial loading of the collection failed", err)
		return err
	}

	service.log.Println("starting to initialize collection", collection)

	template, err := service.CollectionTemplateService.GetTemplate(templateID)

	if err != nil {
		service.log.Println("unable to find template", templateID)
	}

	service.log.Println("starting to apply template", template)

	tx := service.database.MustBegin()

	if name != nil {
		service.log.Println("update template name", name)
		err := service.UpdateName(collectionID, *name, tx)

		if err != nil {
			service.log.Println("unable to set collection name", err)
			tx.Rollback()
			return err
		}
	}

	for _, column := range template.Columns {
		service.log.Println("trying to create column", column)
	}

	service.log.Println("committing collection initialization to database")
	err = tx.Commit()

	if err != nil {
		service.log.Println("unable to commit transaction for collection initialization", err)
		tx.Rollback()

		return err
	}

	return nil
}
