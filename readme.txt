Google Safe Browsing API v2.0 C#, release 0.2.0, May 01 2011
-----------------------------------------------------------------

https://github.com/OndrejStastny/Google-Safe-Browsing-API-2.0-C-

1. INTRODUCTION
----------------

This is the README file for Google Safe Browsing API v2.0 C#, C# implementation of protocol
available at http://code.google.com/apis/safebrowsing/developers_guide_v2.html.

Licensed under the Apache License, Version 2.0 (the "License"); you may not 
use this file except in compliance with the License. You may obtain a copy 
of the License at 
 
    http://www.apache.org/licenses/LICENSE-2.0 


2. KNOWN ISSUES
---------------

Does not support MAC authentication.
REKEY not supported.
Only supports 32bit key sizes.


3. RELEASE INFO
----------------

This is the first release that is not yet ready for production. There is still
a bug in data persistence. Data are being duplicated and insert to DB fails.
More unit tests should be created to get better coverage. Missing functionality 
will be added soon.