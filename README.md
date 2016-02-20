# Dianoga for Kentico

An automatic image optimizer for the Kentico media library, forked from [Dianoga for Sitecore project, build by kamsar](https://github.com/kamsar/Dianoga). Reduce the size of your images served from Kentico by 8-40%, completely automatically.

When media images are uploaded, Dianoga automatically runs [jpegtran](http://jpegclub.org/jpegtran/) or [PNGOptimizer](http://psydk.org/pngoptimizer) on the image data immediately just before it is saved in the Kentico media library. 

Dianoga ensures that your site is always serving fully optimised media library images even if you are using Kenticos's dynamic resizing features.

Dianoga is also great for situations where content editors may not be image editing experts and upload images that contain gobs of EXIF data and other nonessential metadata - these are removed automatically before being shown to visitors. All of the optimizations done are lossless (no quantizing, etc) so you lose no image quality.

## Performance

Dianoga runs synchronously _before_ the image is saved into the media library. The performance of Dianoga is logged into the Kentico log. PNGs are very fast because it's a native P/Invoke call to a C DLL - about 20ms for a small one. JPEGs result in executing a program and are slightly slower - around 200ms for a medium sized image. Larger images several MB in size can take a second or two depending on the hardware in use.

Once the image is in cache for the requested size, there is no performance penalty as the file is read from the file system unless it is changed again.

## Limitations

Because Dianoga uses a DLL version of PNGOptimizer, it is platform-specific and only runs on 64-bit application pools.

Dianoga depends on the Dianoga Tools folder that is installed by the NuGet package into the web project it is installed on. You can relocate these tools if you wish by changing the paths in the `web.config` file, inside "Dianoga" section.

## Installation

Dianoga has a NuGet package for Kentico 8.x (.NET 4) and for Kentico 9 (.Net 4.6). Just install it and you're done.

The code should compile against Kentico without issue.

To perform a manual installation:

* Copy the Dianoga Tools folder to the root of your website
* Add web.config section:
	<configSections>
		<section name="dianoga" type="Dianoga.Config.DianogaSection" />
	</configSections>
	<dianoga>
    	<optimizers>
      		<add type="Dianoga.Png.PngOptimizer, Dianoga" path="~/Dianoga Tools/PNGOptimizer/PNGOptimizerDll.dll" />
      		<add type="Dianoga.Jpeg.JpegOptimizer, Dianoga" path="~/Dianoga Tools/libjpeg/jpegtran.exe"/>
		</optimizers>
	</dianoga>
* Reference Dianoga.dll or the source project in your web project

## Troubleshooting

If you're not seeing optimization take place, there are a couple of possibilities:

* An error is occurring. The Kentico event logs catch all errors that occur when generating a media stream, so look there first.

## Extending Dianoga

You can define your own optimizers for additional media types if you wish, in the `web.config` file, inside "Dianoga" section

This software is based in part on the work of the Independent JPEG Group