package models

import (
	"database/sql"
)

type User struct {
	ID        string         `db:"id"`
	Email     string         `db:"email"`
	GivenName string         `db:"givenname"`
	SurName   sql.NullString `db:"surname"`
}
