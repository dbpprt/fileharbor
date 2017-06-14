package services

import (
	"log"

	"github.com/dennisbappert/fileharbor/common"
	"github.com/jmoiron/sqlx"
	minio "github.com/minio/minio-go"
)

type StorageService struct {
	Service
	storage *minio.Client
}

func NewStorageService(configuration *common.Configuration, database *sqlx.DB, storage *minio.Client, services *Services) *StorageService {
	service := &StorageService{Service{database: database, configuration: configuration, Services: services}, storage}
	return service
}

func (service *StorageService) CreateBucket(name string) error {
	log.Println("creating new bucket", name)
	err := service.storage.MakeBucket(name, service.configuration.Storage.Region)

	if err != nil {
		log.Println("unable to create bucket", name)
		return err
	}

	return nil
}

func (service *StorageService) DeleteBucket(name string, force bool) error {
	log.Println("deleting bucket name, force", name, force)

	// TODO: delete bucket only if empty
	err := service.storage.RemoveBucket(name)

	if err != nil {
		log.Println("unable to delete bucket", name)
		return err
	}

	return nil
}
