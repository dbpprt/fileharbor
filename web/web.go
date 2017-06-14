package web

import (
	"log"
	"net/http"
	"time"

	"github.com/dennisbappert/fileharbor/common"
	"github.com/dennisbappert/fileharbor/web/context"
	uuid "github.com/satori/go.uuid"

	"github.com/labstack/echo"
	"github.com/labstack/echo/middleware"

	"github.com/dennisbappert/fileharbor/services"
)

func Initialize(configuration *common.Configuration, services *services.Services) {
	e := echo.New()
	e.HideBanner = true

	// we always want to have a trailing slash
	e.Pre(middleware.AddTrailingSlash())

	// create a request id for all requests
	e.Use(middleware.RequestIDWithConfig(middleware.RequestIDConfig{
		Generator: func() string {
			// use uuids as request ids
			return uuid.NewV4().String()
		},
	}))

	// register our custom context to avoid package global variables
	e.Use(func(handlerFunc echo.HandlerFunc) echo.HandlerFunc {
		return func(echoContext echo.Context) error {
			// TODO: debug this code to find out the actual behaviour
			// TODO: add current request context to the object: -> current user & collection & request id
			ctx, err := context.New(&echoContext, configuration, services)

			if err != nil {
				return err
			}

			return handlerFunc(ctx)

			// TODO: add a logger instance which includes a unique request id
		}
	})

	e.Use(middleware.Logger())
	e.Use(middleware.Recover())

	e.Use(middleware.GzipWithConfig(middleware.GzipConfig{
		Level: -1,
	}))

	configureRoutes(e)

	// TODO: read from config
	// TODO: graceful shutdown

	log.Println("starting http server", configuration.Addr)
	e.Logger.Fatal(e.StartServer(&http.Server{
		Addr:         configuration.Addr,
		ReadTimeout:  time.Duration(configuration.ReadTimeout) * time.Minute,
		WriteTimeout: time.Duration(configuration.WriteTimeout) * time.Minute,
	}))
}
