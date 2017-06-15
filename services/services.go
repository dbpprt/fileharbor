package services

import (
	"log"

	"os"

	"github.com/dennisbappert/fileharbor/common"
	"github.com/jmoiron/sqlx"
	minio "github.com/minio/minio-go"
	uuid "github.com/satori/go.uuid"
)

type ServiceEnvironment struct {
	CurrentUserId           string
	Email                   string
	CurrentUserIsSuperAdmin bool
	RequestId               string
	// TODO: custom logger
}

type ServiceContext struct {
	database      *sqlx.DB
	configuration *common.Configuration
	storage       *minio.Client

	log *log.Logger

	Environment *ServiceEnvironment

	UserService               *UserService
	CollectionService         *CollectionService
	StorageService            *StorageService
	CollectionTemplateService *CollectionTemplateService
	ColumnService             *ColumnService
}

type Service struct {
	*ServiceContext

	// TODO: this is probably not necessary as *ServiceContext should mixin these fields
	database      *sqlx.DB
	configuration *common.Configuration
}

func (serviceContext *ServiceContext) NewServiceContext(environment *ServiceEnvironment) *ServiceContext {
	ctx := NewServiceContext(serviceContext.configuration, environment, serviceContext.database, serviceContext.storage)
	return ctx
}

func NewServiceContext(configuration *common.Configuration, environment *ServiceEnvironment, database *sqlx.DB, storage *minio.Client) *ServiceContext {
	ctx := &ServiceContext{}

	ctx.configuration = configuration
	ctx.database = database
	ctx.storage = storage

	ctx.Environment = environment
	ctx.UserService = NewUserService(configuration, database, ctx)
	ctx.CollectionService = NewCollectionService(configuration, database, ctx)
	ctx.StorageService = NewStorageService(configuration, database, storage, ctx)
	ctx.CollectionTemplateService = NewCollectionTemplateService(configuration, database, ctx)
	ctx.ColumnService = NewColumnService(configuration, database, ctx)

	if ctx.Environment != nil {
		ctx.log = log.New(os.Stdout, "("+ctx.Environment.RequestId+") ("+environment.Email+") ", log.LstdFlags|log.Ldate|log.Ltime|log.Lshortfile)
	}

	return ctx
}

func NewAnonymousEnvironment(requestId string) *ServiceEnvironment {
	return &ServiceEnvironment{
		CurrentUserId: uuid.Nil.String(),
		Email:         "anonymous",
		CurrentUserIsSuperAdmin: false,
		RequestId:               requestId,
	}
}

func NewUserEnvironment(requestId string, userId string, email string, superAdmin bool) *ServiceEnvironment {
	return &ServiceEnvironment{
		CurrentUserId: uuid.Nil.String(),
		Email:         "anonymous",
		CurrentUserIsSuperAdmin: false,
		RequestId:               requestId,
	}
}

func NewSystemEnvironment() *ServiceEnvironment {
	return &ServiceEnvironment{
		CurrentUserId:           uuid.Nil.String(),
		CurrentUserIsSuperAdmin: true,
		RequestId:               uuid.Nil.String(),
	}
}
