package handlers

import (
	"log"
	"net/http"

	"github.com/dennisbappert/fileharbor/web/context"
	"github.com/labstack/echo"
)

func HomeIndex(c echo.Context) error {
	ctx := c.(*context.Context)
	db := ctx.Database()
	log.Println(db)
	return c.String(http.StatusOK, "Hello, World!\n")
}
