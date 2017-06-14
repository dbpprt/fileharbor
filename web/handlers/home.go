package handlers

import (
	"net/http"

	"github.com/dennisbappert/fileharbor/web/context"
	"github.com/labstack/echo"
)

func HomeIndex(c echo.Context) error {
	ctx := c.(*context.Context)
	ctx.CollectionTemplateService.GetAvaliableTemplates("en-US")

	return c.String(http.StatusOK, "Hello, World!\n")
}
