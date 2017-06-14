package common

import (
	"encoding/json"
	"os"
)

type Configuration struct {
	Port             int
	ConnectionString string
}

func (configuration *Configuration) Parse(path *string) error {
	file, err := os.Open(*path)

	if err != nil {
		return err
	}

	decoder := json.NewDecoder(file)
	err = decoder.Decode(&configuration)

	if err != nil {
		return err
	}

	return nil
}
