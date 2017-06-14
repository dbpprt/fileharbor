package main

import (
	"flag"
	"log"

	"github.com/dennisbappert/fileharbor/common"

	"github.com/dennisbappert/fileharbor/database"
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
	log.Println("connection successful")

	if *webFlag {
		log.Println("starting web interface")
		web.Initialize(&configuration, db)
	}

	defer db.Close()

}
