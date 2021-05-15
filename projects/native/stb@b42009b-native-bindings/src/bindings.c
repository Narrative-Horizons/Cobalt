#define STB_BINDING_EXPORT __declspec(dllexport)

#include <stdbool.h>

#define STB_IMAGE_IMPLEMENTATION
#include <stb_image.h>

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
    //stbi_set_flip_vertically_on_load(true);

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
