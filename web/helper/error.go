package helper

import (
	"github.com/dennisbappert/fileharbor/common"
)

// TODO: add translations for errors

type ErrorMessage struct {
	Code    string `json:"code"`
	Message string `json:"message"`
	Field   string `json:"field,omitempty"`
}

type ErrorResponse struct {
	Messages []ErrorMessage `json:"messages"`
}

func NewEmptyErrorResponse() *ErrorResponse {
	return &ErrorResponse{
		Messages: nil,
	}
}

func NewErrorResponse(code string, message string) *ErrorResponse {
	return &ErrorResponse{
		Messages: []ErrorMessage{
			ErrorMessage{
				Code:    code,
				Message: message,
			},
		},
	}
}

func (errorResponse *ErrorResponse) AddError(code string, message string) {
	errorResponse.Messages = append(errorResponse.Messages, ErrorMessage{
		Code:    code,
		Message: message,
	})
}

func (errorResponse *ErrorResponse) AddFieldError(code string, message string, field string) {
	errorResponse.Messages = append(errorResponse.Messages, ErrorMessage{
		Code:    code,
		Message: message,
		Field:   field,
	})
}

func NewUnexpectedErrorResponse() *ErrorResponse {
	return &ErrorResponse{
		Messages: []ErrorMessage{
			ErrorMessage{
				Code:    common.ErrUnexpected,
				Message: "Sorry an unexpected error occurred :( Please try again later...",
			},
		},
	}
}
