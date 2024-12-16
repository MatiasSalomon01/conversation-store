pipeline {
    agent any
    environment {
        GIT_URL = 'https://gitea.clt.com.py/CLT/ms-clt-conversation-store.git'
        GIT_CREDENTIALS_ID = 'e6c8151a-46e4-4f87-8ecc-4b4fbe829237'
        GIT_URL_DEPLOYMENT = 'https://gitea.clt.com.py/CLT/ci-cd-deployment-k8s.git'
 
        DOCKER_CREDENTIALS_ID = '42ec3223-0710-4cb5-87c3-cb4c17192bd8'
        DOCKER_IMAGE = 'ms-clt-conversation-store'
        DOCKER_REGISTRY = 'registry.clt.com.py/crux'
        DOCKER_IMAGE_VERSION = 'latest'
        DOCKER_IMAGE_TAG = "${env.BUILD_NUMBER}"
 
        SONAR_TOKEN = credentials('af486eb9-2bf3-49ed-9489-987fc72d39c4')
        SONAR_HOST_URL = 'https://sonar.clt.com.py'
        SCANNER_HOME = tool name: 'SonarScanner.NET'
        SONAR_PROJECT_KEY = 'ms-clt-conversation-store'
    }

    triggers {
        GenericTrigger(
            genericVariables: [
                [key: 'ref', value: '$.ref']
            ],
            printContributedVariables: true,
            token: 'ms-clt-conversation-store',
            printPostContent: true,
            regexpFilterText: '$ref',
           regexpFilterExpression: '^refs/heads/(main|dev|qa)$'
        )
    }
 
    stages {
        stage('Validate Branch') {
            steps {
                script {
                    def branchName = env.ref?.replace('refs/heads/', '') ?: 'unknown'
                    env.GIT_BRANCH_NAME  = branchName
                    echo "Triggered by branch: ${branchName}"
                }
            }
        }

        stage('Project Clone Repository') {
            steps {
                git branch: env.GIT_BRANCH_NAME,
                    credentialsId: env.GIT_CREDENTIALS_ID,
                    url: env.GIT_URL
            }
        }
 
        stage('Restore') {
            steps {
                sh 'dotnet restore'
            }
        }
 
        stage('Unit Test') {
            steps {
               echo 'Running unit tests... work in progress'
            }
        }
 
        stage('Functional Test') {
            steps {
                echo 'Running functional tests... work in progress'
            }
        }
 
        stage('File System Scan') {
            steps {
                sh "trivy fs --format table -o trivy-fs-report.html ."
            }
        }
 
        stage('SonarQube Analysis') {
            steps {
                withSonarQubeEnv('SonarQube') {
                    sh """
                        apt-get update && apt-get install -y default-jre
                        dotnet /var/jenkins_home/tools/hudson.plugins.sonar.MsBuildSQRunnerInstallation/SonarScanner.NET/SonarScanner.MSBuild.dll begin /k:${env.SONAR_PROJECT_KEY} /d:sonar.host.url=https://sonar.clt.com.py/ /d:sonar.login=${env.SONAR_TOKEN}
                        dotnet build
                        dotnet /var/jenkins_home/tools/hudson.plugins.sonar.MsBuildSQRunnerInstallation/SonarScanner.NET/SonarScanner.MSBuild.dll end /d:sonar.login=${env.SONAR_TOKEN}
                    """
                }
            }
        }
 
        stage('Quality Gate') {
            steps {
                script {
                    catchError(buildResult: 'SUCCESS', stageResult: 'UNSTABLE') {
                        waitForQualityGate abortPipeline: false, credentialsId: env.SONAR_TOKEN
                    }
                }
            }
        }
 
        stage('Increment Version Image') {
            steps {
                script {
                    withCredentials([usernamePassword(credentialsId: env.DOCKER_CREDENTIALS_ID,
                                                      passwordVariable: 'DOCKER_PASSWORD',
                                                      usernameVariable: 'DOCKER_USERNAME')]) {
                        sh "docker login -u ${DOCKER_USERNAME} -p ${DOCKER_PASSWORD} ${env.DOCKER_REGISTRY}"
                        def latestTag = sh(
                           script: '''
                                #!/bin/bash
                                docker_username="${DOCKER_USERNAME}"
                                docker_password="${DOCKER_PASSWORD}"
                                docker_registry="${DOCKER_REGISTRY}"
 
                                curl -s -u "$docker_username:$docker_password" "https://$docker_registry/api/v2.0/projects/crux/repositories/ms-clt-conversation-store/tags"
                            ''',
                            returnStdout: true
                        );
                        echo "Last tag: ${latestTag}"
                    }
                }
            }
        }
 
        stage('Building Docker') {
            steps {
                script {
                    def dockerImageWithTag = "${env.DOCKER_IMAGE}:${env.DOCKER_IMAGE_TAG}"
                    def dockerImageLatestTag = "${env.DOCKER_IMAGE}:${env.DOCKER_IMAGE_VERSION}"

                    sh "docker build  -t ${dockerImageWithTag} --no-cache ."
                    sh "docker build  -t ${dockerImageLatestTag} --no-cache ."
                }
            }
        }
 
         stage('Docker Image Scan') {
            steps {
                script {
                    def scanResult = sh(
                        script: "trivy image --exit-code 1 --severity HIGH,CRITICAL --format table -o trivy-image-report.html ${env.DOCKER_IMAGE}:${env.DOCKER_IMAGE_TAG}",
                        returnStatus: true
                    )
                    if (scanResult != 0) {
                        if (env.GIT_BRANCH_NAME == 'dev') {
                            echo "Vulnerabilities found, but pipeline will continue because branch is '${env.GIT_BRANCH_NAME}'."
                        } else {
                            error("Pipeline aborted: vulnerabilities found.")
                        }
                    }
                }
            }
        }
 
        stage('Push Docker Images') {
            steps {
                script {
                    withCredentials([usernamePassword(credentialsId: env.DOCKER_CREDENTIALS_ID,
                            passwordVariable: 'DOCKER_PASSWORD',
                            usernameVariable: 'DOCKER_USERNAME')]) {
                        sh "docker login -u ${DOCKER_USERNAME} -p ${DOCKER_PASSWORD} ${env.DOCKER_REGISTRY}"
                    }

                    def dockerImageWithTag = "${env.DOCKER_IMAGE}:${env.DOCKER_IMAGE_TAG}"
                    def dockerImageLatestTag = "${env.DOCKER_IMAGE}:${env.DOCKER_IMAGE_VERSION}"

                    sh "docker tag ${dockerImageWithTag} ${env.DOCKER_REGISTRY}/${dockerImageWithTag}"
                    sh "docker tag ${dockerImageLatestTag} ${env.DOCKER_REGISTRY}/${dockerImageLatestTag}"

                    sh "docker push ${env.DOCKER_REGISTRY}/${dockerImageWithTag}"
                    sh "docker push ${env.DOCKER_REGISTRY}/${dockerImageLatestTag}"
                }
            }
        }
 
        stage('Clone & Commit Deployment Repository') {
            steps {
                git branch: env.GIT_BRANCH_NAME,
                    credentialsId: env.GIT_CREDENTIALS_ID,
                    url: env.GIT_URL_DEPLOYMENT

                script {
                    withCredentials([usernamePassword(credentialsId: env.GIT_CREDENTIALS_ID, usernameVariable: 'GIT_USERNAME', passwordVariable: 'GIT_PASSWORD')]) {
                        sh """
                            cd crux/ms-clt-conversation-store

                            git config --global user.email 'jenkins@clt.com.py'
                            git config --global user.name 'jenkins'

                            sed -i 's|image: registry.clt.com.py/crux/ms-clt-conversation-store:[^ ]*|image: registry.clt.com.py/crux/ms-clt-conversation-store:${BUILD_NUMBER}|g' deployment.yaml

                            git add deployment.yaml
                            git commit -m 'Updated App Deployment | Jenkins Pipeline'
                            
                            git push https://${GIT_USERNAME}:${GIT_PASSWORD}@${env.GIT_URL_DEPLOYMENT.replace('https://', '')} ${env.GIT_BRANCH_NAME}
                        """
                    }
                }
            }
        }
    }

    post {
        always {
            script {
                def jobName = env.JOB_NAME
                def buildNumber = env.BUILD_NUMBER
                def pipelineStatus = currentBuild.result ?: 'UNKNOWN'
                def bannerColor = pipelineStatus.toUpperCase() == "SUCCESS" ? 'green' : 'red'
 
                def body = """<html>
                    <body>
                        <div style="border: 4px solid ${bannerColor}; padding: 10px;">
                            <h2>${jobName} - Build ${buildNumber}</h2>
                            <div style="background-color: ${bannerColor}; padding: 10px;">
                            <h3 style="color: white;">Pipeline Status: ${pipelineStatus.toUpperCase()}</h3>
                            </div>
                            <p>Check the <a href="${BUILD_URL}">console output</a>.</p>
                         </div>
                    </body>
                </html>"""
 
                emailext (
                    subject: "${jobName} - Build ${buildNumber} - ${pipelineStatus.toUpperCase()}",
                    body: body,
                    mimeType: 'text/html',
                    to: 'jose.mclanghlin@clt.com.py,matias.salomon@clt.com.py',
                    from: 'alertas.seguridad@clt.com.py',
                    replyTo: 'elmer.benitez@clt.com.py',
                    attachmentsPattern: 'trivy-image-report.html'
                )
 
                def dockerImageWithTag = "${env.DOCKER_IMAGE}:${env.DOCKER_IMAGE_TAG}"
                def dockerImageLatestTag = "${env.DOCKER_IMAGE}:${env.LATEST_TAG}"
 
                sh "docker images -q ${dockerImageWithTag} | grep -q . && docker rmi --force ${dockerImageWithTag} || echo 'Image does not exist ${dockerImageWithTag}'"
                sh "docker images -q ${dockerImageLatestTag} | grep -q . && docker rmi  --force ${dockerImageLatestTag} || echo 'Image does not exist ${dockerImageLatestTag}'"
 
                cleanWs()
            }
        }
    }
}