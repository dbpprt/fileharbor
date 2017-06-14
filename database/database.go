package database

import (
	"github.com/dennisbappert/fileharbor/common"

	"github.com/jmoiron/sqlx"
	_ "github.com/lib/pq"
)

func Initialize(configuration *common.Configuration) (*sqlx.DB, error) {
	db, err := sqlx.Open("postgres", configuration.ConnectionString)

	if err != nil {
		return nil, err
	}

	if err = db.Ping(); err != nil {
		return nil, err
	}

	return db, nil
}
