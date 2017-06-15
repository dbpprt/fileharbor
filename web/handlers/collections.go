package handlers

import (
	"log"
	"net/http"

	"github.com/dennisbappert/fileharbor/common"
	"github.com/dennisbappert/fileharbor/web/context"
	"github.com/dennisbappert/fileharbor/web/helper"
	"github.com/labstack/echo"
)

func CollectionsGetTemplates(c echo.Context) error {
	ctx := c.(*context.Context)

	type template struct {
		ID          string `json:"id"`
		Name        string `json:"name"`
		Description string `json:"description"`
		Language    string `json:"language"`
	}

	templates, err := ctx.CollectionTemplateService.GetAvaliableTemplates()

	if err != nil {
		if applicationError, ok := err.(*common.ApplicationError); ok {
			response := helper.NewErrorResponse(applicationError.Code, applicationError.Error())
			return c.JSON(http.StatusOK, response)
		}

		log.Println("unexpected error occurred - sending 500", err)
		response := helper.NewUnexpectedErrorResponse()
		return c.JSON(http.StatusInternalServerError, response)
	}

	var results []template
	for _, current := range *templates {
		results = append(results, template{
			ID:          current.ID,
			Name:        current.Name,
			Description: current.Description,
			Language:    current.Language,
		})
	}

	return c.JSON(http.StatusOK, results)
}

func CollectionsGetMy(c echo.Context) error {
	ctx := c.(*context.Context)

	type collection struct {
		ID string `json:"id"`
	}

	collections, err := ctx.CollectionService.MyCollections()

	if err != nil {
		if applicationError, ok := err.(*common.ApplicationError); ok {
			response := helper.NewErrorResponse(applicationError.Code, applicationError.Error())
			return c.JSON(http.StatusOK, response)
		}

		log.Println("unexpected error occurred - sending 500", err)
		response := helper.NewUnexpectedErrorResponse()
		return c.JSON(http.StatusInternalServerError, response)
	}

	var results []collection
	for _, current := range *collections {
		results = append(results, collection{
			ID: current.ID,
		})
	}

	return c.JSON(http.StatusOK, results)
}
