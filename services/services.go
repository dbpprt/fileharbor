package services

import (
	"github.com/dennisbappert/fileharbor/common"
	"github.com/jmoiron/sqlx"
	minio "github.com/minio/minio-go"
)

type Services struct {
	UserService               *UserService
	CollectionService         *CollectionService
	StorageService            *StorageService
	CollectionTemplateService *CollectionTemplateService
}

type Service struct {
	*Services

	database      *sqlx.DB
	configuration *common.Configuration
}

func Initialize(configuration *common.Configuration, database *sqlx.DB, mc *minio.Client) *Services {
	services := &Services{}
	services.UserService = NewUserService(configuration, database, services)
	services.CollectionService = NewCollectionService(configuration, database, services)
	services.StorageService = NewStorageService(configuration, database, mc, services)
	services.CollectionTemplateService = NewCollectionTemplateService(configuration, database, services)
	return services
}
