package services

import (
	"log"
	"math/rand"
	"strings"
	"time"

	"golang.org/x/crypto/bcrypt"

	"database/sql"

	"github.com/dennisbappert/fileharbor/common"
	"github.com/dennisbappert/fileharbor/models"
	"github.com/jmoiron/sqlx"
	uuid "github.com/satori/go.uuid"
)

type UserService struct {
	Service
}

func normalizeEmail(email string) string {
	return strings.ToLower(email)
}

func validateEmail(email string) error {
	// TODO: validate email address
	return nil
}

func validatePassword(password string) error {
	if len(password) < 8 {
		return common.NewApplicationError("The password should have 8 or more characters", common.ErrPasswordTooShort)
	}

	return nil
}

func NewUserService(configuration *common.Configuration, database *sqlx.DB, services *Services) *UserService {
	service := &UserService{Service{database: database, configuration: configuration, Services: services}}
	return service
}

func (service *UserService) Exists(email string) (bool, error) {
	user := models.UserEntity{}
	email = normalizeEmail(email)

	// TODO: logging in this func is crap, make it better

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

func (service *UserService) Login(email string, password string) error {
	log.Println("trying to login user", email)

	// TODO: validate mail while logging in? maybe an issue while updathing the validateEmail function
	// if err := validateEmail(email); err != nil {
	// 	log.Println("error while validating email")
	// 	return "", err
	// }

	user := models.UserEntity{}
	email = normalizeEmail(email)

	// TODO: logging in this func is crap, make it better

	log.Println("looking up user", email)
	err := service.database.Get(&user, "SELECT password_hash FROM users where email=$1", email)

	// TODO: thhis looks like bullshit, there should be a better way
	if err == nil {
		log.Println("user found in database", email)

		// bcrypt takes a long time, so we better hide this fast operation
		time.Sleep(time.Duration(rand.Intn(450)) * time.Millisecond)

		if err := bcrypt.CompareHashAndPassword(user.PasswordHash, []byte(password)); err != nil {
			// TODO: maybe strip some characters of the log?
			log.Println("failed login attempt", email, password) // this log is just to identify possible bruteforce attacks
		} else {
			log.Println("user succesfully logged in", email)
			return nil
		}
	} else if err != nil && err != sql.ErrNoRows {
		log.Println("unexpected error while logging in user", email)
		return err
	}

	// bcrypt takes a long time, so we better hide this fast operation - bcrypt takes about 80ms on a core i7 energy saver in curacao at about 30 degress ;)
	time.Sleep(time.Duration(rand.Intn(450)) * time.Millisecond)

	return common.NewApplicationError("Unable to login", common.ErrLoginFailed)
}

func (service *UserService) Register(email string, givenname string, password string) (string, error) {
	log.Println("registering new user", email)

	if err := validateEmail(email); err != nil {
		log.Println("error while validating email")
		return "", err
	}

	if exists, err := service.Exists(email); err != nil {
		return "", err
	} else if exists == true {
		log.Println("stopping user creation because user is already existing")
		return "", common.NewApplicationError("user already exists", common.ErrUserAlreadyExisting)
	}

	if err := validatePassword(password); err != nil {
		log.Println("error while validating password")
		return "", err
	}

	// generate a new user id
	id := uuid.NewV4().String()
	email = normalizeEmail(email)

	// password hashing
	hash, err := bcrypt.GenerateFromPassword([]byte(password), bcrypt.DefaultCost)
	if err != nil {
		log.Println("unable to create password hash - aborting! - never should happen?", password)
		return "", err
	}

	log.Println("beginning insert", id, email)

	tx := service.database.MustBegin()
	_, err = tx.Exec("INSERT INTO users (id, email, givenname, password_hash) VALUES($1, $2, $3, $4)", id, email, givenname, hash)

	if err != nil {
		log.Println("error while creating user", err)
		tx.Rollback()
		return "", err
	}

	log.Println("user successfully created", id)

	// TODO: move all the creation stuff to the user validated event
	// TODO: send mail to user
	// TODO: the received link should validate the account and start with the creation
	collection, err := service.CollectionService.Create(tx)

	if err != nil {
		log.Println("unable to create collection, rolling back user creation")
		tx.Rollback()
		return "", err
	}

	log.Println("assigning user to newly created collection")
	err = service.CollectionService.AssignUser(id, collection, tx)

	if err != nil {
		log.Println("unable to assigning user to collection, rolling back user creation")
		tx.Rollback()
		return "", err
	}

	log.Println("committing user creation to database")
	err = tx.Commit()
	if err != nil {
		log.Println("unable to commit transaction for user creation - deleting bucket", err)
		tx.Rollback()

		cleanupErr := service.StorageService.DeleteBucket(collection, false)
		if cleanupErr != nil {
			log.Println("unable to cleanup bucket, please delete manually", collection, cleanupErr)
		}

		return "", err
	}

	return id, nil
}
