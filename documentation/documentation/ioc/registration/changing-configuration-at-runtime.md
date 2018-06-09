<!--Title: Changing Configuration at Runtime-->

<[warning]>
Don't do this, you've been warned.
<[/warning]>

The Lamar team respectfully recommends that you don't do this, but this functionality is here because we had to have this for
Jasper's integration with ASP.Net Core. Please note that this should only be used **additively**. Unlike StructureMap, Lamar will not rewrite build
plans for existing registrations to accomodate changes here. 

<[sample:add_all_new_services]>

