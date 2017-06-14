package web

import (
	"net/http"

	"github.com/dennisbappert/fileharbor/common"

	"github.com/labstack/echo"
	"github.com/labstack/echo/middleware"
)

func Initialize(configuration *common.Configuration) {
	e := echo.New()
	e.HideBanner = true

	e.Use(middleware.Logger())
	e.Use(middleware.Recover())

	e.GET("/", func(c echo.Context) error {
		return c.String(http.StatusOK, "Hello, World!\n")
	})

	e.Logger.Fatal(e.Start(":1323"))
}
