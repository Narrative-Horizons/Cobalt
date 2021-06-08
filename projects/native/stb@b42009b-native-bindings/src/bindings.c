#define STB_BINDING_EXPORT __declspec(dllexport)

#include <stdbool.h>

#define STB_IMAGE_IMPLEMENTATION
#include <stb_image.h>
#define STB_IMAGE_WRITE_IMPLEMENTATION
#include <stb_image_write.h>

typedef struct _ImagePayload
{
    unsigned char*  sdr_ub_image;
    unsigned short* sdr_us_image;
    float*          hdr_f_image;
    int    width;
    int    height;
    int    channels;
} ImagePayload;

static void cobalt_empty_initialize(ImagePayload* img)
{
    img->sdr_ub_image = NULL;
    img->sdr_us_image = NULL;
    img->hdr_f_image  = NULL;
    img->width        = 0;
    img->height       = 0;
    img->channels     = 0;
}

STB_BINDING_EXPORT bool cobalt_load_image(const char* filename, ImagePayload* payload)
{
    int desired_channels;
    int is_hdr;
    int is_16_bit;

    desired_channels = 4;
    is_hdr = stbi_is_hdr(filename);
    is_16_bit = stbi_is_16_bit(filename);
    stbi_set_flip_vertically_on_load(true);

    payload->channels = desired_channels;

    int channels_in_file;

    if (is_hdr)
    {
        payload->hdr_f_image = stbi_loadf(filename, &payload->width, &payload->height, &channels_in_file, desired_channels);
    }
    else if (is_16_bit)
    {
        payload->sdr_us_image = stbi_load_16(filename, &payload->width, &payload->height, &channels_in_file, desired_channels);
    }
    else
    {
        payload->sdr_ub_image = stbi_load(filename, &payload->width, &payload->height, &channels_in_file, desired_channels);
    }

    if (payload->hdr_f_image == NULL && payload->sdr_ub_image == NULL && payload->sdr_us_image == NULL)
    {
#ifdef _DEBUG
        int errnum;
        errnum = errno;

        printf("Error: %s\n", strerror(errnum));
#endif
        return false;
    }

    return true;
}

STB_BINDING_EXPORT void cobalt_release_image(ImagePayload* payload)
{
    if (payload->hdr_f_image != NULL)
    {
        stbi_image_free(payload->hdr_f_image);
    }
    if (payload->sdr_ub_image != NULL)
    {
        stbi_image_free(payload->sdr_ub_image);
    }
    if (payload->sdr_us_image != NULL)
    {
        stbi_image_free(payload->sdr_us_image);
    }

    // Upon the completion of payload emptying, reset the struct to an initial value
    cobalt_empty_initialize(payload);
}

STB_BINDING_EXPORT void cobalt_save_image_as_png(const char* path, ImagePayload* payload)
{
    if(payload->sdr_ub_image != NULL)
        stbi_write_png(path, payload->width, payload->height, payload->channels, payload->sdr_ub_image, payload->channels * sizeof(unsigned char) * payload->width);
    if(payload->sdr_us_image != NULL)
        stbi_write_png(path, payload->width, payload->height, payload->channels, payload->sdr_ub_image, payload->channels * sizeof(unsigned short) * payload->width);
}

STB_BINDING_EXPORT int cobalt_combine_ORM_image(const char* roughnessMetallicPath, const char* occlusionPath, ImagePayload* ORMPayload)
{
    ImagePayload* roughnessMetallicData = malloc(sizeof(ImagePayload));
    ImagePayload* occlusionData = malloc(sizeof(ImagePayload));

    cobalt_load_image(roughnessMetallicPath, roughnessMetallicData);
    cobalt_load_image(occlusionPath, occlusionData);

    stbi_flip_vertically_on_write(true);

    // RED: AO
    // GREEN: Roughness
    // BLUE: Metallic

    if (roughnessMetallicData->sdr_ub_image != NULL && occlusionData->sdr_ub_image != NULL)
    {
        // unsigned byte conversion
        int channels = 4;
        int size = sizeof(unsigned char*) * roughnessMetallicData->width * roughnessMetallicData->height * channels;

        unsigned char* ORMData = malloc(size);

        for (int i = 0; i < roughnessMetallicData->width * roughnessMetallicData->height; i++)
        {
            ORMData[i * channels + 0] = occlusionData->sdr_ub_image[i * occlusionData->channels + 0];
            ORMData[i * channels + 1] = roughnessMetallicData->sdr_ub_image[i * roughnessMetallicData->channels + 1];
            ORMData[i * channels + 2] = roughnessMetallicData->sdr_ub_image[i * roughnessMetallicData->channels + 2];
            ORMData[i * channels + 3] = 255;
        }

        ORMPayload->channels = channels;
        ORMPayload->width = roughnessMetallicData->width;
        ORMPayload->height = roughnessMetallicData->height;
        ORMPayload->sdr_ub_image = ORMData;

        return 0;
    }
    else if (roughnessMetallicData->sdr_us_image != NULL && occlusionData->sdr_us_image != NULL)
    {
        // unsiged short conversion
        int channels = 4;
        int size = sizeof(unsigned short*) * roughnessMetallicData->width * roughnessMetallicData->height * channels;

        unsigned short* ORMData = malloc(size);

        for (int i = 0; i < roughnessMetallicData->width * roughnessMetallicData->height; i++)
        {
            ORMData[i * channels + 0] = occlusionData->sdr_ub_image[i * occlusionData->channels + 0];
            ORMData[i * channels + 1] = roughnessMetallicData->sdr_ub_image[i * roughnessMetallicData->channels + 1];
            ORMData[i * channels + 2] = roughnessMetallicData->sdr_ub_image[i * roughnessMetallicData->channels + 2];
            ORMData[i * channels + 3] = 65535;
        }

        ORMPayload->channels = channels;
        ORMPayload->width = roughnessMetallicData->width;
        ORMPayload->height = roughnessMetallicData->height;
        ORMPayload->sdr_us_image = ORMData;

        return 0;
    }
    else
    {
        // HDR or mismatch, return error
        return -1;
    }

    return 1;
}