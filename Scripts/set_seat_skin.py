from PIL import Image, ImageDraw, ImageFilter
import cv2
import numpy as np
import sys


def rounded_rectangle(draw, xy, corner_radius, fill):
    x1, y1, x2, y2 = xy
    draw.rectangle([x1, y1 + corner_radius, x2, y2 - corner_radius], fill=fill)
    draw.rectangle([x1 + corner_radius, y1, x2 - corner_radius, y2], fill=fill)
    draw.pieslice(
        [x1, y1, x1 + corner_radius * 2, y1 + corner_radius * 2], 180, 270, fill=fill
    )
    draw.pieslice(
        [x2 - corner_radius * 2, y1, x2, y1 + corner_radius * 2], 270, 360, fill=fill
    )
    draw.pieslice(
        [x1, y2 - corner_radius * 2, x1 + corner_radius * 2, y2], 90, 180, fill=fill
    )
    draw.pieslice(
        [x2 - corner_radius * 2, y2 - corner_radius * 2, x2, y2], 0, 90, fill=fill
    )


def add_rounded_border(
    input_image_array,
    border_left,
    border_top,
    border_right,
    border_bottom,
    corner_radius,
):
    # 打开图片
    image = Image.fromarray(input_image_array)

    # 获取图片大小
    image_size = image.size

    # 创建一个带有透明背景的新图片
    new_image = Image.new("RGBA", image_size, (0, 0, 0, 0))

    # 创建一个圆角蒙版
    mask = Image.new("L", image.size, 0)
    draw = ImageDraw.Draw(mask)
    draw.rounded_rectangle(
        (
            border_left,
            border_top,
            image.width - border_right,
            image.height - border_bottom,
        ),
        corner_radius,
        fill=255,
    )

    # 将原始图片贴在圆角矩形内
    new_image.paste(image, (0, 0), mask)

    # 创建一个与原矩形尺寸相同的灰色圆角矩形背景在左上方
    bg_image = Image.new("RGBA", image_size, (44, 23, 21, 255))
    mask1 = Image.new("L", image.size, 0)
    draw = ImageDraw.Draw(mask1)
    draw.rounded_rectangle(
        (
            border_left + 5,
            border_top - 2,
            image.width - border_right - 5,
            image.height - border_bottom - 2,
        ),
        corner_radius,
        fill=255,
    )
    # 对 alpha 通道进行高斯模糊
    feathered_alpha = mask1.filter(ImageFilter.GaussianBlur(0.5))

    # 合并原始图像的 RGB 通道和羽化后的 alpha 通道
    mask1 = Image.merge("RGBA", image.split()[:3] + (feathered_alpha,))
    new_image1 = Image.new("RGBA", image_size, (0, 0, 0, 0))
    new_image1.paste(bg_image, (0, 0), mask1)
    # bg_image.paste(bg_image)
    new_image1.paste(new_image, (0, 0), mask)
    # new_image1.paste(new_image, (0, 0))

    # 保存结果图片
    return new_image1


large_height, large_width = 500, 500
template_height, template_width = 270, 229


def match(alpha, gray_template, scale):
    gray_alpha = cv2.cvtColor(alpha, cv2.COLOR_RGBA2GRAY)
    gray_alpha = cv2.resize(
        gray_alpha, (int(large_width * scale), int(large_height * scale))
    )

    # 在背景中找到透明通道的位置
    result = cv2.matchTemplate(gray_alpha, gray_template, cv2.TM_CCORR)
    # result = cv2.matchTemplate(gray_alpha, gray_template, cv2.TM_CCORR)
    _, max_value, _, max_loc = cv2.minMaxLoc(result)
    # print(max_value)

    return max_value, max_loc


def remove_background(template, alpha):
    # template = cv2.imread(template_path, cv2.IMREAD_UNCHANGED)
    gray_template = cv2.cvtColor(template, cv2.COLOR_RGBA2GRAY)

    # 读取背景图片和带有透明通道的图片
    # alpha = cv2.imread(alpha_path, cv2.IMREAD_UNCHANGED)
    alpha_height, alpha_width = alpha.shape[:2]

    empty_image = np.zeros((500, 500, 4), dtype=np.uint8)

    # 获取大图片和小图片的宽度和高度

    # 计算小图片在大图片中心的坐标
    start_y = (large_height - alpha_height) // 2
    end_y = start_y + alpha_height
    start_x = (large_width - alpha_width) // 2
    end_x = start_x + alpha_width

    # 将小图片贴在大图片中心
    empty_image[start_y:end_y, start_x:end_x] = alpha
    alpha = empty_image

    # min_scale, max_scale = 0.68, 0.9
    max_value = 0
    loc = None
    p = 0.9

    for scale in np.arange(0.6, 1.0, 0.01):
        v, l1 = match(alpha, gray_template, scale)
        if v > max_value:
            max_value = v
            loc = l1
        else:
            break

    # while True:
    #     v1, l1 = match(alpha, gray_template, min_scale)
    #     v2, l2 = match(alpha, gray_template, max_scale)
    #     if v1 - v2 < 0.002 and v1 - v2 > -0.002:
    #         loc = l2
    #         break
    #     # print(min_scale)
    #     # print(max_scale)
    #     # scale = (min_scale + max_scale) / 2
    #     if v1 > v2:
    #         max_scale = max_scale * p + min_scale * (1 - p)
    #     else:
    #         min_scale = max_scale * (1 - p) + min_scale * p
    print(scale)

    alpha = cv2.resize(alpha, (int(large_width * scale), int(large_height * scale)))

    x1, y1 = loc[0], loc[1]
    x2, y2 = x1 + template_width, y1 + template_height

    # cv2.rectangle(alpha, (x1, y1), (x2, y2), 255, 2)
    # cv2.imshow("image", alpha)
    # cv2.waitKey()
    # exit()

    alpha = alpha[y1:y2, x1:x2]
    output1 = np.zeros((22, template_width, 4), dtype=np.uint8)
    # output1[:, :, 0] = template[:, :, 2]
    # output1[:, :, 1] = template[:, :, 1]
    output1[:, :, :3] = template[:22, :, :3]
    output1[:, :, 3] = alpha[:22, :, 3]
    # cv2.imwrite(output_path, output1)
    return output1


# 用法示例
input_image_path = (
    "Assets/StreamingAssets/Image/General/Seat/" + sys.argv[1] + ".png"
)  # 替换为你的输入图片路径
alpha_path = (
    "Assets/StreamingAssets/Image/General/Window/" + sys.argv[1] + ".png"
)  # 替换为带有透明通道的人物图片的路径
output_image_path = input_image_path + ".png"  # 替换为你的输出图片路径
border_left = 20
border_top = 19
border_right = 20
border_bottom = 2
corner_radius = 8
feather_size = 10

image0 = template = cv2.imread(input_image_path, cv2.IMREAD_UNCHANGED)
image0 = cv2.resize(image0, (229, 270))
image0 = cv2.cvtColor(image0, cv2.COLOR_BGRA2RGBA)

alpha = cv2.imread(alpha_path, cv2.IMREAD_UNCHANGED)
alpha = cv2.cvtColor(alpha, cv2.COLOR_BGRA2RGBA)

image = add_rounded_border(
    image0,
    border_left,
    border_top,
    border_right,
    border_bottom,
    corner_radius,
)


image1 = Image.fromarray(remove_background(image0, alpha))
# 逐行增加透明度
# for y in range(0, 3, 1):
#     # 从原图中截取一行
#     row = image1.crop((0, y, template_width, y + 1))
#     # 计算透明度
#     alpha = int(y * 255 / 3)
#     # 添加透明度到新图像
#     row.putalpha(alpha)
#     image1.paste(row, (0, y), image1.crop((0, y, template_width, y + 1)))

image.paste(image1, (0, 0), image1)
image.save(output_image_path, format="PNG")
