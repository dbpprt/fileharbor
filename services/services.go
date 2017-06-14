package services

import (
	"github.com/dennisbappert/fileharbor/common"
	"github.com/jmoiron/sqlx"
)

type Services struct {
	UserService       *UserService
	CollectionService *CollectionService
}

func Initialize(configuration *common.Configuration, database *sqlx.DB) *Services {
	services := &Services{}
	services.UserService = NewUserService(configuration, database, services)
	services.CollectionService = NewCollectionService(configuration, database, services)

	return services
}
