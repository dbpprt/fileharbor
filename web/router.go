package web

import (
	"github.com/dennisbappert/fileharbor/web/handlers"

	"github.com/dennisbappert/fileharbor/common"
	"github.com/labstack/echo"
)

func configureRoutes(e *echo.Echo, anonymous *echo.Group, authenticated *echo.Group, superadmin *echo.Group, configuration *common.Configuration) error {

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
	authenticated.GET("/collections/my/", handlers.CollectionsGetMy)
	authenticated.POST("/collections/name/", handlers.CollectionsUpdateName)
	authenticated.POST("/collections/initialize/", handlers.CollectionsInitialize)

	return nil
}
