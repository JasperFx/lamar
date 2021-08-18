import{o as n,c as a,a as s,d as t,e,b as p}from"./app.80913d4a.js";const o='{"title":"Using Attributes for Configuration","description":"","frontmatter":{},"headers":[{"level":2,"title":"Attribute Targeting Plugin Type or Concrete Type","slug":"attribute-targeting-plugin-type-or-concrete-type"},{"level":2,"title":"Built In Attributes","slug":"built-in-attributes"}],"relativePath":"guide/ioc/registration/attributes.md","lastUpdated":1629293895173}',c={},i=t("h1",{id:"using-attributes-for-configuration"},[t("a",{class:"header-anchor",href:"#using-attributes-for-configuration","aria-hidden":"true"},"#"),e(" Using Attributes for Configuration")],-1),l=t("p",null,[e("The Lamar team believe that forcing users to spray .Net attributes all over their own code is in clear violation of our philosophy of minimal intrusion into user code. "),t("em",null,"In other words, we don't want to be MEF.")],-1),r=t("p",null,[e("That being said, there are plenty of times when simple attribute usage is effective for one-off deviations from your normal registration conventions or cause less harm than having to constantly change a centralized "),t("code",null,"ServerRegistry"),e(" or just seem more clear and understandable to users than a convention. For those usages, Lamar 4.0 has introduced a new base class that can be extended and used to explicitly customize your Lamar configuration:")],-1),u=p('',7),k=p('',4),m=p('',8);c.render=function(t,e,p,o,c,d){return n(),a("div",null,[i,l,r,s(" snippet: sample_LamarAttribute "),u,s(" snippet: sample_SingletonAttribute "),k,s(" snippet: sample_[Singleton]-usage "),m])};export{o as __pageData,c as default};
