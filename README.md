## AppLocale Alternative Starter

#### What is it?

Short version: it lets you run Japanese games.  
Long version: it's an alternative frontend for Microsoft AppLocale. You can set any locale you want for application that was created without Unicode support, thus allowing them to be executed as they were meant to.

*It still requires AppLocale to be installed, but it will prompt you if it wasn't found.*

Consider this example. Here user tries to run ゆめにっき (Yume Nikki) without locale fix:

![1](https://f.cloud.github.com/assets/1045476/2070957/6074d840-8d22-11e3-8a0d-d882f32ffa67.gif)

...and here user creates shortcut with AppLocale Alternative Starter, that makes the game run properly.

![2](https://f.cloud.github.com/assets/1045476/2070956/6074cfbc-8d22-11e3-981a-11b5e0a36137.gif)

---

#### Why alternative? What are the benefits?

##### Better layout

The default AppLocale frontend looks like this:

![applocale](https://f.cloud.github.com/assets/1045476/2070960/68e4e4a2-8d22-11e3-8d05-ac84eef29971.png)

...while AppLocale Alternative Starter looks like this:

![tmp](https://f.cloud.github.com/assets/1045476/2071083/04a8ddac-8d24-11e3-960f-2d913d6d45d8.png)

##### Intuitive

Default wizard will make shortcut, but it won't say where it was put. (Oh, it was in Start Menu. How obvious.) With Alternative Starter you decide where to put that shortcut, so that you don't have to search for it afterwards. Additionally, it will detect if you have AppLocale engine installed in first place - if not, it will suggest installing it and redirect you directly to Microsoft's download page.

##### No nagging

Even though you can create a shortcut that automatically runs your application in desired locale, default AppLocale will warn you every time you try to run your application:

>AppLocale is a temporary solution for non-Unicode applications.
>
>If you are commonly using non-Unicode applications in a given language, it is strongly recommended to properly set your system's "Language For Non-Unicode Programs" variable.

##### No more ¥

If you follow Microsoft's advice and set your global locale to Japanese, you eventually get lots of most ridiculous problems. One of them is that every backslash in your system will be drawn using yen character. [No, really.](http://en.wikipedia.org/wiki/%C2%A5) Thus, using AppLocale to restrict locale change to just one process, makes it better solution.

##### You can set current working directory

I don't know if you would ever need it, but anyway - you *can* do it, whereas it is not possible to do with default AppLocale frontend whatsoever. I can imagine some applications requiring this feature, seeing that people still create locale-specific applications...
