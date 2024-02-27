pipeline {
    agent any
    environment {
        AWS_ACCESS_KEY_ID     = credentials('AWSKEY')
        AWS_SECRET_ACCESS_KEY = credentials('AWSSECRET')
    }
    stages {
        stage('Deploy To ECR') {
            steps {
                sh 'aws ecs update-service --cluster todolist --service API --desired-count 1 --force-new-deployment'
            }
        }
    }
}
