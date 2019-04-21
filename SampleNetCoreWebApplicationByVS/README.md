# ����.Net Core��Visual Studio������Asp.Net��Ŀģ��

# �汾
����.net core 2.x

## ��Դռ��
.net core��linux�µ��ڴ�ռ�ã�����ʱ��Լ50M���ҡ�����һ�κ����ߵ�70M����

## ��Ŀ���
* SampleWebApplicationEmpty
����Ŀģ��

* SampleWebApplication
WebӦ��ģ��

* SampleWebApplicationApi
API��Ŀģ��

* SampleWebApplicationMvc
������MVC��Ŀģ��

* SampleWebApplicationWithAngular
MVC + Angular ��Ŀģ��

## ��Ŀ������
ÿ����Ŀ����һ��launchSettings.json�����������������Ŀʹ�ú�����������
```json
{
  "iisSettings": {			//IIS������
    "windowsAuthentication": false, 		//�Ƿ�ʹ��windows�����֤
    "anonymousAuthentication": true, 		//�Ƿ��������˾���ͨ��
    "iisExpress": {
      "applicationUrl": "http://localhost:63704",		//http��ַ�Ͷ˿�
      "sslPort": 44315		//������SSL�˿����ǿ�ƿ�ʼSSL
    }
  },
  "profiles": {
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "SampleWebApplicationMvc": {		//�Խ��̷�ʽ����(�Դ�http������)
      "commandName": "Project",
      "launchBrowser": true,
      "applicationUrl": "https://localhost:5001;http://localhost:5000",		//http��https�Ķ˿�
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```


## ��Ŀ������
��Ŀ�����������Program.cs������Ҫ�Ĳ�ͬ��Startup.cs��