package common

import (
	"encoding/json"
	"os"
)

type Configuration struct {
	Addr             string `json:"addr"`
	ConnectionString string `json:"connection_string"`
	ReadTimeout      int    `json:"read_timeout"`
	WriteTimeout     int    `json:"write_timeout"`
	DefaultQuota     int    `json:"default_quota"`

	Token struct {
		Lifetime int    `json:"lifetime"`
		Secret   string `json:"secret"`
	} `json:"token"`

	Storage struct {
		AccessKey string `json:"access_key"`
		SecretKey string `json:"secret_key"`
		Region    string `json:"region"`
		Endpoint  string `json:"endpoint"`
		UseSSL    bool   `json:"use_ssl"`
	} `json:"storage"`

	TemplateFolder string `json:"template_folder"`
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
