# SonarQube Report

SonarQube Report is a console application which connects to a SonarQube server instance. Gets different configurable metric values from the rest API, creates a custom html report and send it by email.

## Email template

You can customize the email send by the tool through an email template. A html file `email-template.html` in the same directory as the application.

Metric values are masked with `{{[metric-key]}}`, where the metric key are [standard metric names defined by SonarQube]([https://docs.sonarqube.org/latest/user-guide/metric-definitions/]()). For example: `{{new_vulnerabilities}}`

The rest is plain html.

## How to configure the application

### SonarQubeProjectKey

The SonarQube project key. For example: "test-project"

### SonarQubeApiUrl

The SonarQube api url. [sonarqube-server-url]/api. For example: "http://mysonarqubeserver/api/"

### SonarQubeLastAnalysisUri

SonarQube project analysis endpoint. For example: 
"project_analyses/search?project={0}&ps=1"

> This value should rarely change.

### SonarQubeMetricUri

SonarQube project metrics endpoint. For example: 
"measures/component?component={0}&metricKeys={1}"

> This value should rarely change.

### SonarQubeMetrics

A comma separated list of all the SonarQube metrics you want to include in the report. 

For example: "bugs,new_bugs,vulnerabilities,new_vulnerabilities,sqale_index,new_technical_debt,code_smells,new_code_smells,tests,test_execution_time,coverage,new_coverage,duplicated_lines_density,duplicated_blocks"

> For a complete list of metrics you can review [https://docs.sonarqube.org/latest/user-guide/metric-definitions/]()

### SonarQubeUserName

SonarQube application user name. The user should have read access rights on the project to be analized.

### SonarQubePassword

SonarQube application user password.

### SmtpHost

Smtp email server. For example for gmail is `smtp.gmail.com`

### SmtpPort

Smtp email server port. For example for gmail is `587`

### SmtpUserName

Smtp email server user name.

### SmtpPassword

Smtp email server user password.

### SmtpTo

Email address to send report.

### SmtpEnableSsl

Smtp server enable ssl configuration.

### SmtpUseDefaultCredentials

Smtp server use defaults credentials configuration.

### ReportsDirectory

Where the html reports are stored.  

## How to run the application

    dotnet SonarQube.Report.dll

Optionally you can run it with the flag `/noemail` to run all the process, generate the html report but skip the email send.

    dotnet SonarQube.Report.dll -- /noemail

## Output

You will get output in the same console about the status of the process. As a final step it will try to send an email to the configured email addresses.

Also in the configured reports directory it will create a html file (same content as the email) with the following name pattern.

    [date]_[hour]_report.html

    // for example

    2019-25-02_03:51:39PM_report.html
