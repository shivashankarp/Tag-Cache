Tag Cache
=========

Tag Cache is a full fledged cache that also enables associating tags (aka labels) 
with cache items. The tags can later be used to invalidate multiple cache items 
that match the tag in one go.

Example
-------

    TagCache tagCache = new TagCache();
    
    Action fillCacheItems = delegate
    {
        tagCache.Set("honda", new object(), new List<string> { "Vehicle", "Car", "Economy" });
        tagCache.Set("lexus", new object(), new List<string> { "Vehicle", "Car", "Luxury" });
        tagCache.Set("harley", new object(), new List<string> { "Vehicle", "Bike", "Luxury" });
        tagCache.Set("yamaha", new object(), new List<string> { "Vehicle", "Bike", "Economy" });
    };
    
    fillCacheItems();
    tagCache.Invalidate(new List<List<string>> { new List<string> { "Bike" } });  // invalidates all bikes
    
    fillCacheItems();
    tagCache.Invalidate(new List<List<string>> { new List<string> { "Luxury" } });  // invalidates all luxury vehicles
    
    fillCacheItems();
    tagCache.Invalidate(new List<List<string>> { new List<string> { "Vehicle" } });  // invalidates all vehicles
    
    fillCacheItems();
    tagCache.Invalidate(new List<List<string>> { new List<string> { "Car", "Luxury" } });  // invalidates all luxury cars
    
    fillCacheItems();
    tagCache.Invalidate(new List<List<string>> { new List<string> { "Bike", "Economy" } });  // invalidates all economy bikes

    fillCacheItems();
    tagCache.Invalidate(new List<List<string>> { 
        new List<string> { "Bike", "Luxury" }, 
        new List<string> { "Car", "Economy" } 
    });  // invalidates all luxury bikes, economy cars

License
-------

(The MIT License)

Copyright (c) 2011 Shiva Shankar P

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
'Software'), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

