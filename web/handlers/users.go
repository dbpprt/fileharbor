package handlers

import (
	"net/http"

	"github.com/dennisbappert/fileharbor/common"
	"github.com/dennisbappert/fileharbor/web/context"
	"github.com/dennisbappert/fileharbor/web/helper"
	"github.com/labstack/echo"
)

func UsersRegister(c echo.Context) error {
	ctx := c.(*context.Context)

	type registration struct {
		GivenName string `json:"givenname"`
		SurName   string `json:"surname"`
		Email     string `json:"email"`
	}

	id, err := ctx.Services.UserService.Register("admin@admin.io", "Bappert", "Dennis")

	if err != nil {
		if applicationError, ok := err.(*common.ApplicationError); ok {
			response := helper.NewErrorResponse(applicationError.Code, applicationError.Error())
			return c.JSON(http.StatusOK, response)
		}

		panic(err)
	}

	return c.String(http.StatusOK, id)
}
