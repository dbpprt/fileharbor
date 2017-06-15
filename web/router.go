package web

import (
	"github.com/dennisbappert/fileharbor/web/handlers"

	"github.com/dennisbappert/fileharbor/common"
	"github.com/labstack/echo"
	"github.com/labstack/echo/middleware"
)

func configureRoutes(e *echo.Echo, configuration *common.Configuration) error {
	// our default auth middleware borrowed from echo ;)
	authMiddleware := middleware.JWT([]byte(configuration.Token.Secret))

	anonymous := e.Group("")
	authenticated := e.Group("", authMiddleware)
	//superadmin := e.Group("") // not implemented yet :(

	// static files
	anonymous.File("/", "public/index.html")
	anonymous.File("/favicon.ico", "public/images/favicon.ico")
	anonymous.Static("/public", "public")

	// home.go
	//anonymous.GET("/", handlers.HomeIndex)

	// users.go
	anonymous.POST("/users/register/", handlers.UsersRegister)
	anonymous.POST("/users/login/", handlers.UsersLogin)

	// collections.go
	authenticated.GET("/collections/templates/", handlers.CollectionsGetTemplates)

	return nil
}
