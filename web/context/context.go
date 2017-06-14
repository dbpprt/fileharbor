package context

import (
	"github.com/dennisbappert/fileharbor/common"
	"github.com/dennisbappert/fileharbor/services"
	"github.com/jmoiron/sqlx"
	"github.com/labstack/echo"
)

type Context struct {
	echo.Context

	database *sqlx.DB

	Services *services.Services
}

func New(c *echo.Context, configuration *common.Configuration, db *sqlx.DB) (*Context, error) {
	services := services.Initialize(configuration, db)

	ctx := &Context{Context: *c, database: db, Services: services}
	return ctx, nil
}
