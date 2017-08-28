package services

import (
	"database/sql"
	"encoding/json"

	"strings"

	"github.com/dennisbappert/fileharbor/common"
	"github.com/dennisbappert/fileharbor/models"
	"github.com/jmoiron/sqlx"
)

type ColumnService struct {
	Service
}

type Column struct {
	ID          string
	Name        string
	Description string
	Group       string
	Type        string
	Sealed      bool
	Settings    json.RawMessage
}

type columnSettings struct{}

type TextColumnSettings struct {
	*columnSettings
	MaxLength int `json:"max-length"`
}

func NewColumnService(configuration *common.Configuration, database *sqlx.DB, services *ServiceContext) *ColumnService {
	service := &ColumnService{Service{database: database, configuration: configuration, ServiceContext: services}}
	return service
}

func (service *ColumnService) validateAndNormalizeTextColumn(column *Column) error {

	return nil
}

func (service *ColumnService) validateAndNormalizeDateTimeColumn(column *Column) error {

	return nil
}

func (service *ColumnService) Exists(id string, collectionID string) (bool, error) {
	column := models.ColumnEntity{}
	service.log.Println("looking up column", id)
	err := service.database.Get(&column, "SELECT id FROM columns where id=$1 and collection_id=$2", id, collectionID)

	// TODO: thhis looks like bullshit, there should be a better way
	if err != nil && err == sql.ErrNoRows {
		service.log.Println("column is not existing")
		return false, nil
	} else if err != nil {
		return true, err
	}

	service.log.Println("column is existing", column)
	return true, nil
}

func (service *ColumnService) Create(column *Column, collectionID string, tx *sqlx.Tx) error {
	var err error

	service.log.Println("creating column", column)
	service.log.Println("in target collection", collectionID)

	// check if the collection exists
	if exists, err := service.CollectionService.Exists(collectionID, tx); err != nil {
		service.log.Println("unable to check if collection exists - aborting...", err)
		return err
	} else if !exists {
		service.log.Println("collection is not existing - aborting...", collectionID)
		return common.NewApplicationError("Collection is not existing", common.ErrNotFound)
	}

	// check if the column may already exists
	if exists, err := service.Exists(column.ID, collectionID); err != nil {
		service.log.Println("unable to check if column exists - aborting...", err)
		return err
	} else if exists {
		service.log.Println("column is existing - aborting...", column.ID)
		return common.NewApplicationError("The column is already existing", common.ErrColumnAlreadyExists)
	}

	switch columnType := strings.ToLower(column.Type); columnType {
	case "text":
		err = service.validateAndNormalizeTextColumn(column)

	case "datetime":
		err = service.validateAndNormalizeDateTimeColumn(column)

	default:
		return common.NewApplicationError("Undefined column type", common.ErrUnknownColumnType)
	}

	if err != nil {
		service.log.Println("unable to validate column", column)
		service.log.Println("aborting creation")
		return err
	}

	commit := false
	if tx == nil {
		tx = service.database.MustBegin()
		commit = true
	}

	_, err = tx.Exec("INSERT INTO columns (id, collection_id, name, description, \"group\", type, sealed, settings) VALUES ($1, $2, $3, $4, $5, $6, $7, $8)",
		column.ID, collectionID, column.Name, column.Description, column.Group, column.Type, column.Sealed, column.Settings)

	if err != nil {
		service.log.Println("unexpected error creating column", err)

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

	return nil
}
