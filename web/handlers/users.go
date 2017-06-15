package handlers

import (
	"log"
	"net/http"
	"time"

	"github.com/dennisbappert/fileharbor/common"
	"github.com/dennisbappert/fileharbor/web/context"
	"github.com/dennisbappert/fileharbor/web/helper"
	jwt "github.com/dgrijalva/jwt-go"
	"github.com/labstack/echo"
)

func UsersLogin(c echo.Context) error {
	ctx := c.(*context.Context)

	type login struct {
		Email    string `json:"email"`
		Password string `json:"password"`
	}

	req := new(login)
	if err := c.Bind(req); err != nil {
		log.Println("unable to bind request body to type login")
		return c.NoContent(http.StatusBadRequest)
	}

	user, err := ctx.UserService.Login(req.Email, req.Password)

	if err != nil {
		if applicationError, ok := err.(*common.ApplicationError); ok {
			response := helper.NewErrorResponse(applicationError.Code, applicationError.Error())
			return c.JSON(http.StatusOK, response)
		}

		log.Println("unexpected error occurred - sending 500", err)
		response := helper.NewUnexpectedErrorResponse()
		return c.JSON(http.StatusInternalServerError, response)
	}

	claims := &helper.Claims{
		ID:         user.ID,
		Email:      user.Email,
		SuperAdmin: user.SuperAdmin,
		StandardClaims: jwt.StandardClaims{
			ExpiresAt: time.Now().Add(time.Hour * time.Duration(ctx.Configuration.Token.Lifetime)).Unix(),
		},
	}

	token := jwt.NewWithClaims(jwt.SigningMethodHS256, claims)

	// generate encoded token and send it as response.
	signedString, err := token.SignedString([]byte(ctx.Configuration.Token.Secret))
	if err != nil {
		return err
	}
	return c.JSON(http.StatusOK, map[string]string{
		"token": signedString,
	})
}

// TODO: password
// TODO: validate mail
func UsersRegister(c echo.Context) error {
	ctx := c.(*context.Context)

	type registration struct {
		GivenName string `json:"givenname"`
		Email     string `json:"email"`
		Password  string `json:"password"`
	}

	reg := new(registration)
	if err := c.Bind(reg); err != nil {
		log.Println("unable to bind request body to type restration")
		return c.NoContent(http.StatusBadRequest)
	}

	id, err := ctx.UserService.Register(reg.Email, reg.GivenName, reg.Password)

	if err != nil {
		if applicationError, ok := err.(*common.ApplicationError); ok {
			response := helper.NewErrorResponse(applicationError.Code, applicationError.Error())
			return c.JSON(http.StatusOK, response)
		}

		log.Println("unexpected error occurred - sending 500", err)
		response := helper.NewUnexpectedErrorResponse()
		return c.JSON(http.StatusInternalServerError, response)
	}

	return c.JSON(http.StatusOK, struct {
		Id string `json:"id"`
	}{id})
}
