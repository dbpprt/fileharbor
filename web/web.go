package web

import (
	"log"
	"net/http"
	"time"

	"github.com/dennisbappert/fileharbor/common"
	"github.com/dennisbappert/fileharbor/web/context"
	jwt "github.com/dgrijalva/jwt-go"
	uuid "github.com/satori/go.uuid"

	"github.com/labstack/echo"
	"github.com/labstack/echo/middleware"

	"github.com/dennisbappert/fileharbor/services"
	"github.com/dennisbappert/fileharbor/web/helper"
)

func Initialize(configuration *common.Configuration, serviceContext *services.ServiceContext) {
	e := echo.New()
	e.HideBanner = true

	// we always want to have a trailing slash
	e.Pre(middleware.AddTrailingSlash())

	// create a request id for all requests
	e.Use(middleware.RequestIDWithConfig(middleware.RequestIDConfig{
		Generator: func() string {
			// use uuids as request ids
			return uuid.NewV4().String()
		},
	}))
	// TODO: set request id as response header!

	// TODO: implement auto tls

	if configuration.DebugMode {
		log.Println("enabling unrestricted cors for debugging purposes!")
		e.Use(middleware.CORS())

		log.Println("enabling echo debug mode")
		e.Debug = true
	}

	// register our custom context to avoid package global variables
	e.Use(func(handlerFunc echo.HandlerFunc) echo.HandlerFunc {
		return func(echoContext echo.Context) error {
			// TODO: debug this code to find out the actual behaviour - done!
			// TODO: create the services instance here to place a custom context into it with a custom logger, the current user and the current request id
			// TODO: add current request context to the object: -> current user & collection & request id
			// TODO: add a logger instance which includes a unique request id

			// were passing a new service context from the existing one but with an empty serviceenvironemnt which means => anonymous user
			serviceContext.UserService.LogContext()
			ctx, err := context.New(&echoContext, configuration, serviceContext.NewServiceContext(nil))
			ctx.UserService.LogContext()
			if err != nil {
				return err
			}

			return handlerFunc(ctx)
		}
	})

	e.Use(middleware.Logger())
	e.Use(middleware.Recover())

	e.Use(middleware.GzipWithConfig(middleware.GzipConfig{
		Level: -1,
	}))

	// authentication middleware
	config := middleware.JWTConfig{
		Claims:     &helper.Claims{},
		SigningKey: []byte(configuration.Token.Secret),
	}
	authMiddleware := middleware.JWTWithConfig(config)

	contextMiddleware := func(handlerFunc echo.HandlerFunc) echo.HandlerFunc {
		return func(echoContext echo.Context) error {
			// get request id
			requestId := echoContext.Response().Header().Get(echo.HeaderXRequestID)
			log.Println("looking up request id for incomming request", requestId)

			var currentServiceContext *services.ServiceContext = nil
			if token, ok := echoContext.Get("user").(*jwt.Token); ok {
				log.Println("fetched context from context", token)

				claims := token.Claims.(*helper.Claims)
				log.Println("extracted user identity from signed token", claims.Email)

				currentServiceContext = serviceContext.NewServiceContext(nil)
			} else {
				log.Println("no valid token and identitfy found")
			}

			if currentServiceContext == nil {
				currentServiceContext = serviceContext.NewServiceContext(nil)
			}

			ctx, err := context.New(&echoContext, configuration, currentServiceContext)

			if err != nil {
				log.Println("unable to set context", err)
				return err
			}

			return handlerFunc(ctx)
		}
	}

	// build our different groups
	anonymous := e.Group("", contextMiddleware)
	authenticated := e.Group("", authMiddleware, contextMiddleware)
	superadmin := e.Group("", authMiddleware, contextMiddleware) // TODO: not implemented yet :(

	configureRoutes(e, anonymous, authenticated, superadmin, configuration)

	// TODO: graceful shutdown

	log.Println("starting http server", configuration.Addr)
	e.Logger.Fatal(e.StartServer(&http.Server{
		Addr:         configuration.Addr,
		ReadTimeout:  time.Duration(configuration.ReadTimeout) * time.Second,
		WriteTimeout: time.Duration(configuration.WriteTimeout) * time.Second,
	}))
}
