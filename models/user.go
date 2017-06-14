package models

import (
	"database/sql"
)

type UserEntity struct {
	ID           string         `db:"id"`
	Email        string         `db:"email"`
	GivenName    string         `db:"givenname"`
	SurName      sql.NullString `db:"surname"`
	PasswordHash []byte         `db:"password_hash"`
	SuperAdmin   bool           `db:"superadmin"`
	Validated    bool           `db:"validated"`
}
