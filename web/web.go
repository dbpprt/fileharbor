package web

import (
	"net/http"
	"time"

	"github.com/dennisbappert/fileharbor/common"
	"github.com/dennisbappert/fileharbor/web/context"

	"github.com/labstack/echo"
	"github.com/labstack/echo/middleware"

	"github.com/jmoiron/sqlx"
)

func Initialize(configuration *common.Configuration, db *sqlx.DB) {
	e := echo.New()
	e.HideBanner = true

	// register our custom context to avoid package global variables
	e.Use(func(h echo.HandlerFunc) echo.HandlerFunc {
		return func(c echo.Context) error {
			ctx, err := context.New(&c, db)

			if err != nil {
				return err
			}

			return h(ctx)

			// TODO: add a logger instance which includes a unique request id
		}
	})

	e.Use(middleware.Logger())
	e.Use(middleware.Recover())

	configureRoutes(e)

	// TODO: read from config
	// TODO: graceful shutdown

	e.Logger.Fatal(e.StartServer(&http.Server{
		Addr:         configuration.Addr,
		ReadTimeout:  time.Duration(configuration.ReadTimeout) * time.Minute,
		WriteTimeout: time.Duration(configuration.WriteTimeout) * time.Minute,
	}))
}
