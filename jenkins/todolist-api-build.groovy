pipeline {
    agent any
    environment {
        AWS_ACCESS_KEY_ID     = credentials('AWSKEY')
        AWS_SECRET_ACCESS_KEY = credentials('AWSSECRET')
    }
    stages {
        stage('Cleanup Dir') {
            steps {
                deleteDir()
            }
        }
        stage('checkout git') {
            steps {
                checkout scmGit(
                    branches: [[name: 'master']],
                    userRemoteConfigs: [[credentialsId:  'git_credential', 
                    url: 'git@github.com:karyen92/ToDoListApi.git']]
                )
            }
        }
        stage('Copy Credentials') {
            steps {
                withCredentials([file(credentialsId: 'prod-setting', variable: 'prodSetting')]) {
                    sh "cp \$prodSetting ToDoListApi/appsettings.Production.json"
                }
            }
        }
        stage('Docker Build') {
            steps {
                 sh 'docker build -t todolistapi:${BUILD_NUMBER} -f Dockerfile .'
            }
        }
        stage('ECR Push') {
            steps {
                sh 'aws ecr get-login-password --region ap-southeast-1 | docker login --username AWS --password-stdin <masked>.dkr.ecr.ap-southeast-1.amazonaws.com'
                sh 'docker tag todolistapi:${BUILD_NUMBER} <masked>.dkr.ecr.ap-southeast-1.amazonaws.com/todolistapi:latest'
                sh 'docker push <masked>.dkr.ecr.ap-southeast-1.amazonaws.com/todolistapi:latest'
            }
        }
    }
}
