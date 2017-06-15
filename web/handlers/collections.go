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
		ID         string  `json:"id"`
		Name       *string `json:"name"`
		TemplateId *string `json:"template_id"`
		IsDefault  bool    `json:"is_default"`
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
		result := collection{
			ID:         current.ID,
			Name:       nil,
			TemplateId: nil,
			IsDefault:  current.IsDefault,
		}

		if current.Name.Valid {
			result.Name = &current.Name.String
		}

		if current.TemplateId.Valid {
			result.TemplateId = &current.TemplateId.String
		}

		results = append(results, result)
	}

	return c.JSON(http.StatusOK, results)
}

func CollectionsUpdateName(c echo.Context) error {
	ctx := c.(*context.Context)

	type params struct {
		ID   string `json:"id"`
		Name string `json:"name"`
	}

	param := new(params)
	if err := c.Bind(param); err != nil {
		log.Println("unable to bind request body")
		return c.NoContent(http.StatusBadRequest)
	}

	err := ctx.CollectionService.UpdateName(param.ID, param.Name, nil)

	if err != nil {
		if applicationError, ok := err.(*common.ApplicationError); ok {
			response := helper.NewErrorResponse(applicationError.Code, applicationError.Error())
			return c.JSON(http.StatusOK, response)
		}

		log.Println("unexpected error occurred - sending 500", err)
		response := helper.NewUnexpectedErrorResponse()
		return c.JSON(http.StatusInternalServerError, response)
	}

	return c.NoContent(http.StatusOK)
}

func CollectionsInitialize(c echo.Context) error {
	ctx := c.(*context.Context)

	type params struct {
		ID         string  `json:"id"`
		Name       *string `json:"name"`
		TemplateID string  `json:"template_id"`
	}

	param := new(params)
	if err := c.Bind(param); err != nil {
		log.Println("unable to bind request body")
		return c.NoContent(http.StatusBadRequest)
	}

	err := ctx.CollectionService.InitializeCollection(param.ID, param.TemplateID, param.Name)

	if err != nil {
		if applicationError, ok := err.(*common.ApplicationError); ok {
			response := helper.NewErrorResponse(applicationError.Code, applicationError.Error())
			return c.JSON(http.StatusOK, response)
		}

		log.Println("unexpected error occurred - sending 500", err)
		response := helper.NewUnexpectedErrorResponse()
		return c.JSON(http.StatusInternalServerError, response)
	}

	return c.NoContent(http.StatusOK)
}
