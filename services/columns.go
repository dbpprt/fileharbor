package services

import (
	"github.com/dennisbappert/fileharbor/common"
	"github.com/jmoiron/sqlx"
)

type ColumnService struct {
	Service
}

func NewColumnService(configuration *common.Configuration, database *sqlx.DB, services *Services) *ColumnService {
	service := &ColumnService{Service{database: database, configuration: configuration, Services: services}}
	return service
}
