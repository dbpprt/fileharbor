package web

import (
	"github.com/dennisbappert/fileharbor/web/handlers"

	"github.com/labstack/echo"
)

func configureRoutes(e *echo.Echo) error {

	// home.go
	e.GET("/", handlers.HomeIndex)

	// users.go
	e.POST("/users/register/", handlers.UsersRegister)
	e.POST("/users/login/", handlers.UsersLogin)

	// collections.go
	e.GET("/collections/templates/", handlers.CollectionsGetTemplates)

	return nil
}
