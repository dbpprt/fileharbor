package services

import (
	"github.com/dennisbappert/fileharbor/common"
	"github.com/jmoiron/sqlx"
)

type AuthorizationService struct {
	Service
}

func NewAuthorizationService(configuration *common.Configuration, database *sqlx.DB, services *ServiceContext) *AuthorizationService {
	service := &AuthorizationService{Service{database: database, configuration: configuration, ServiceContext: services}}
	return service
}

func (service *AuthorizationService) IsAnonymous() bool {
	return false
}

func (service *AuthorizationService) IsSystem() bool {
	return true
}

func (service *AuthorizationService) IsLoggedInUser() bool {
	return true
}

func (service *AuthorizationService) IsSuperAdmin() bool {
	return true
}

func (service *AuthorizationService) EnsureLoggedInUser() error {
	return nil
}

func (service *AuthorizationService) EnsureCollectionAccess(collectionID string) error {
	return nil
}
