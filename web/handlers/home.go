package handlers

import (
	"net/http"

	"github.com/labstack/echo"
)

func HomeIndex(c echo.Context) error {
	return c.String(http.StatusOK, "Hello, World!\n")
}
