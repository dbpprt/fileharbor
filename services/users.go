package services

import (
	"log"
	"strings"

	"database/sql"

	"github.com/dennisbappert/fileharbor/common"
	"github.com/dennisbappert/fileharbor/models"
	"github.com/jmoiron/sqlx"
	uuid "github.com/satori/go.uuid"
)

type UserService struct {
	database      *sqlx.DB
	configuration *common.Configuration
	Services      *Services
}

func normalizeEmail(email string) string {
	return strings.ToLower(email)
}

func NewUserService(configuration *common.Configuration, database *sqlx.DB, services *Services) *UserService {
	service := &UserService{database: database, configuration: configuration, Services: services}
	return service
}

func (service *UserService) GetAllUsers() ([]models.User, error) {
	users := []models.User{}
	err := service.database.Select(&users, "SELECT * FROM users")
	return users, err
}

func (service *UserService) Exists(email string) (bool, error) {
	user := models.User{}
	email = normalizeEmail(email)

	log.Println("looking up user", email)
	err := service.database.Get(&user, "SELECT * FROM users where email=$1", email)

	// TODO: thhis looks like bullshit, there should be a better way
	if err != nil && err == sql.ErrNoRows {
		log.Println("user is not existing yet")
		return false, nil
	} else if err != nil {
		return true, err
	}

	log.Println("user is already existing", user)
	return true, nil
}

func (service *UserService) Register(email string, surname string, givenname string) (string, error) {
	log.Println("registering new user", email)
	if exists, err := service.Exists(email); err != nil {
		return "", err
	} else if exists == true {
		log.Println("stopping user creation because user is already existing")
		return "", common.NewApplicationError("user already exists", common.ErrUserAlreadyExisting)
	}

	id := uuid.NewV4().String()
	email = normalizeEmail(email)
	log.Println("beginning insert", id, email)

	tx := service.database.MustBegin()
	tx.MustExec("INSERT INTO users (id, email, givenname, surname) VALUES($1, $2, $3, $4)", id, email, surname, givenname)
	err := tx.Commit()

	if err != nil {
		log.Println("error while executing transaction", err)
		return "", err
	}

	log.Println("user successfully created", id)

	collection, err := service.Services.CollectionService.Create()

	if err != nil {
		log.Println("unable to create collection, rolling back user creation")
		tx.Rollback()
		return "", err
	}

	service.Services.CollectionService.AssignUser(id, collection)

	return id, nil
}
