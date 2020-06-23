# KaschusoNotifier

The Kaschuso Notifier notifies you about your latest mark available for confirmation in the 2 million francs project of the Canton of Solothurn.

## Getting Started

These instructions will cover usage information and for the docker container 

### Prerequisities


In order to run this container you'll need docker installed.

* [Windows](https://docs.docker.com/windows/started)
* [OS X](https://docs.docker.com/mac/started/)
* [Linux](https://docs.docker.com/linux/started/)

### Usage

#### docker-compose
Clone the repository:
```shell
git clone https://github.com/noaahh/KaschusoNotifier.git
cd KaschusoNotifier
```

Edit the [docker-compose.yml](docker-compose.yml) (See 'Environment Variables' for further information).
Compose your containers with:
```shell
docker-compose up -d
```

Check if the containers are running.
```shell
docker ps
```

#### Environment Variables

* `url` - The URL to your school specific Kaschuso
* `username` - Kaschuso username
* `password` - Kaschuso password
* `gmailUsername` - Gmail username (Address) for mail delivery
* `gmailPassword` - Gmail password for mail delivery
* `emailRecipient` - The mail address of the recipient of the mails

## Built With

* https://github.com/puppeteer/puppeteer
* https://github.com/cheeriojs/cheerio
* https://github.com/nodemailer/nodemailer

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.
