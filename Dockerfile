FROM golang:1.8.3

RUN curl -sL https://deb.nodesource.com/setup_8.x | bash && \
	apt-get install -y nodejs && \
	apt-get clean && rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*

WORKDIR $GOPATH/src/github.com/dennisbappert/fileharbor
ADD . .
RUN npm -g install webpack && \
	# make web && pwd && rm -r web/tmp web/node_modules web/bower_components && \
	go get . && go install . 
    # && rm -rf $GOPATH/lib $GOPATH/pkg && \
	#(cd $GOPATH/src && ls | grep -v github | xargs rm -r) && \
	#(cd $GOPATH/src/github.com && ls | grep -v dennisbappert | xargs rm -r)

ENV FILEHARBOR_ADDRESS 0.0.0.0:6565
ENTRYPOINT ["fileharbor"]