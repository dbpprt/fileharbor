package main

import (
	"flag"
	"log"

	"github.com/dennisbappert/fileharbor/common"
	minio "github.com/minio/minio-go"

	"github.com/dennisbappert/fileharbor/database"
	"github.com/dennisbappert/fileharbor/services"
	"github.com/dennisbappert/fileharbor/web"
)

// TODO: implement ldflag versioning

var (
	webFlag           = flag.Bool("web", true, "run fileharbor-web")
	telegramFlag      = flag.Bool("telegram", false, "run telegram bot")
	imapSyncFlag      = flag.Bool("imapsync", false, "run imap-sync")
	debugFlag         = flag.Bool("debug", false, "run in debug mode")
	configurationFile = flag.String("config", "./config/config.json", "path to config file")
)

func main() {
	log.Println("parsing config file", *configurationFile)
	configuration := common.Configuration{}
	if err := configuration.Parse(configurationFile); err != nil {
		log.Println("error while parsing configuration")
		panic(err)
	}
	log.Println("parsed configuration", configuration)

	log.Println("connecting to database")
	db, err := database.Initialize(&configuration)
	if err != nil {
		log.Println("error while connecting to database")
		panic(err)
	}
	log.Println("connection to database established successfully")

	log.Println("connecting to storage endpoint", configuration.Storage.Endpoint)
	mc, err := minio.New(configuration.Storage.Endpoint, configuration.Storage.AccessKey, configuration.Storage.SecretKey, configuration.Storage.UseSSL)
	if err != nil {
		log.Println("unable to connect to storage")
		panic(err)
	}
	log.Println("connection to storage established successfully")

	services := services.Initialize(&configuration, db, mc)

	if *webFlag {
		log.Println("starting web interface")
		web.Initialize(&configuration, services)
	}

	defer db.Close()

}
