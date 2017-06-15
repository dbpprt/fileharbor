package services

import (
	"database/sql"
	"encoding/json"
	"log"

	"errors"

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

func (service *ColumnService) Exists(id string, collectionId string) (bool, error) {
	column := models.ColumnEntity{}
	log.Println("looking up column", id)
	err := service.database.Get(&column, "SELECT * FROM columns where id=$1 and collection_id=$2", id, collectionId)

	// TODO: thhis looks like bullshit, there should be a better way
	if err != nil && err == sql.ErrNoRows {
		log.Println("column is not existing")
		return false, nil
	} else if err != nil {
		return true, err
	}

	log.Println("column is existing", column)
	return true, nil
}

func (service *ColumnService) Create(column *Column, collectionId string) (string, error) {
	var err error

	log.Println("creating column", column)
	log.Println("in target collection", collectionId)

	// check if the collection exists
	if exists, err := service.CollectionService.Exists(collectionId); err != nil {
		log.Println("unable to check if collection exists - aborting...", err)
		return "", err
	} else if !exists {
		log.Println("collection is not existing - aborting...", collectionId)
		return "", errors.New("collection is not found") // TODO: add application error for this
	}

	// check if the column may already exists
	if exists, err := service.Exists(column.ID, collectionId); err != nil {
		log.Println("unable to check if column exists - aborting...", err)
		return "", err
	} else if exists {
		log.Println("column is existing - aborting...", column.ID)
		return "", errors.New("column is already existing") // TODO: add application error for this
	}

	switch columnType := strings.ToLower(column.Type); columnType {
	case "text":
		err = service.validateAndNormalizeTextColumn(column)

	case "datetime":
		err = service.validateAndNormalizeDateTimeColumn(column)

	default:
		return "", errors.New("undefined type") // TODO: add application error for this
	}

	if err != nil {
		log.Println("unable to validate column", column)
		log.Println("aborting creation")
		return "", err
	}

	return "", nil
}
