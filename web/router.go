package web

import (
	"github.com/dennisbappert/fileharbor/web/handlers"

	"github.com/labstack/echo"
)

func configureRoutes(e *echo.Echo) error {

	e.GET("/", handlers.HomeIndex)

	return nil
}
